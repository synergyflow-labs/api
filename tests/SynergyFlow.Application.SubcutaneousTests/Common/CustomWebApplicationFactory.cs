using System.Data.Common;

using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Tests.Common.Security;

using MediatR;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

using Testcontainers.PostgreSql;

using Xunit;

namespace SynergyFlow.Application.SubcutaneousTests.Common;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    static CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "1");
        Environment.SetEnvironmentVariable("ASPNETCORE_hostBuilder_reloadConfigOnChange", "false");
        Environment.SetEnvironmentVariable("DOTNET_hostBuilder_reloadConfigOnChange", "false");
    }

    private static readonly PostgreSqlContainer DbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:18.3")
        .WithDatabase("postgres")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCommand("-c", "max_connections=500")
        .Build();

    private static readonly Lazy<Task> StartLazy = new(async () =>
    {
        await DbContainer.StartAsync();
    });

    private readonly FakeTimeProvider _fakeTimeProvider = new(DateTimeOffset.UtcNow);

    private string? _connectionString;

    public FakeTimeProvider GetFakeTimeProvider() => _fakeTimeProvider;

    public async Task InitializeAsync()
    {
        await StartLazy.Value;

        var dbName = $"synergyflow_test_{Guid.NewGuid():N}";
        var baseConnectionString = DbContainer.GetConnectionString();
        var connBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = baseConnectionString,
            ["Database"] = dbName,
            ["Maximum Pool Size"] = 5,
            ["Minimum Pool Size"] = 0,
        };
        _connectionString = connBuilder.ConnectionString;
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public IMediator CreateMediator()
    {
        var serviceScope = Services.CreateScope();
        return serviceScope.ServiceProvider.GetRequiredService<IMediator>();
    }

    public IServiceScope CreateScope()
    {
        return Services.CreateScope();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            foreach (var source in configBuilder.Sources.OfType<FileConfigurationSource>())
            {
                source.ReloadOnChange = false;
            }
        });

        builder.ConfigureServices(services =>
        {
            var userDescriptors = services.Where(d => d.ServiceType == typeof(IUser)).ToList();
            foreach (var d in userDescriptors)
            {
                services.Remove(d);
            }

            services.AddScoped<IUser, TestCurrentUser>();

            var timeProviderDescriptors = services.Where(d => d.ServiceType == typeof(TimeProvider)).ToList();
            foreach (var d in timeProviderDescriptors)
            {
                services.Remove(d);
            }

            services.AddSingleton<TimeProvider>(_fakeTimeProvider);
        });

        builder.UseSetting("PostgresSettings:DefaultConnection", _connectionString);
        builder.UseSetting("PostgresSettings:MaximumPoolSize", "5");
        builder.UseSetting("PostgresSettings:MinimumPoolSize", "0");
        builder.UseSetting("PostgresSettings:ConnectionTimeoutSeconds", "15");
        builder.UseSetting("AutoMigrateDb", "true");
        builder.UseSetting("RateLimiterSettings:DisableRateLimiting", "true");
        builder.UseSetting("JwtSettings:Secret", "SynergyFlow-SuperSecret-Development-Key-Minimum-32-Characters-Long!");
        builder.UseSetting("JwtSettings:Issuer", "SynergyFlow.Api");
        builder.UseSetting("JwtSettings:Audiences:0", "SynergyFlow.Client");
        builder.UseSetting("JwtSettings:ExpiryMinutes", "15");
        builder.UseSetting("JwtSettings:RefreshTokenExpirationDays", "7");
    }
}
