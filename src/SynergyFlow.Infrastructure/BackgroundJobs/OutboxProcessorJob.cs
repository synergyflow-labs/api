using System.Data;
using System.Diagnostics.Metrics;
using System.Text.Json;

using SynergyFlow.Domain.Common;
using SynergyFlow.Infrastructure.Data;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace SynergyFlow.Infrastructure.BackgroundJobs;

public class OutboxProcessorJob(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<OutboxProcessorJob> logger) : BackgroundService
{
    private const string Channel = "outbox_channel";
    private static readonly TimeSpan NotificationTimeout = TimeSpan.FromSeconds(30);

    private static readonly Meter Meter = new("SynergyFlow.Infrastructure.Outbox");
    private static readonly Counter<int> MessagesProcessed = Meter.CreateCounter<int>("outbox.messages.processed");
    private static readonly Counter<int> MessagesFailed = Meter.CreateCounter<int>("outbox.messages.failed");
    private static readonly Counter<int> NotificationsReceived = Meter.CreateCounter<int>("outbox.notifications.received");
    private static readonly Counter<int> FallbackPolls = Meter.CreateCounter<int>("outbox.fallback.polls");
    private static readonly Histogram<double> ProcessingLatencyMs = Meter.CreateHistogram<double>(
        "outbox.processing.latency_ms",
        unit: "ms",
        description: "Time between event creation and successful processing");

    private readonly SemaphoreSlim _signal = new(0, 1);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Outbox processor started (LISTEN/NOTIFY mode).");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await RunListenerLoopAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Listener loop crashed. Reconnecting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), ct);
            }
        }
    }

    private async Task RunListenerLoopAsync(CancellationToken ct)
    {
        var connectionString = configuration.GetSection("PostgresSettings")["DefaultConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The connection string 'DefaultConnection' in 'PostgresSettings' is not configured.");
        }

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"LISTEN \"{Channel}\"";
        await cmd.ExecuteNonQueryAsync(ct);

        conn.Notification += (_, _) =>
        {
            NotificationsReceived.Add(1);

            try
            {
                _signal.Release();
            }
            catch (SemaphoreFullException)
            {
                // Already signaled
            }
        };

        logger.LogInformation("Listening on channel '{Channel}' for outbox notifications.", Channel);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var hasSignal = await _signal.WaitAsync(NotificationTimeout, ct);

                if (ct.IsCancellationRequested)
                {
                    break;
                }

                if (!hasSignal)
                {
                    FallbackPolls.Add(1);

                    if (conn.State != ConnectionState.Open)
                    {
                        break;
                    }
                }

                await ProcessNextMessageAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox message.");
            }
        }
    }

    private async Task ProcessNextMessageAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        var message = await dbContext.OutboxMessages
            .FromSqlRaw("""
                        SELECT * FROM "OutboxMessages"
                        WHERE "ProcessedOnUtc" IS NULL
                        ORDER BY "OccurredOnUtc" ASC
                        LIMIT 1
                        FOR UPDATE SKIP LOCKED
                        """)
            .SingleOrDefaultAsync(ct);

        if (message == null)
        {
            await transaction.RollbackAsync(ct);
            return;
        }

        var occurredOnUtc = message.OccurredOnUtc;

        try
        {
            var assembly = typeof(DomainEvent).Assembly;
            var type = assembly.GetTypes().FirstOrDefault(t => t.Name == message.Type);

            if (type == null)
            {
                throw new Exception($"Unknown event type: {message.Type}");
            }

            var domainEvent = JsonSerializer.Deserialize(message.Content, type);
            if (domainEvent is INotification notification)
            {
                await mediator.Publish(notification, ct);
            }

            message.ProcessedOnUtc = DateTime.UtcNow;

            MessagesProcessed.Add(1);
            ProcessingLatencyMs.Record((DateTime.UtcNow - occurredOnUtc).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process outbox message {MessageId}", message.Id);
            message.ProcessedOnUtc = DateTime.UtcNow;
            message.Error = ex.ToString();

            MessagesFailed.Add(1);
        }

        await dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
