using System.Data.Common;

using Microsoft.AspNetCore.Mvc.Testing;

using Testcontainers.PostgreSql;

using Xunit;

namespace SynergyFlow.Api.IntegrationTests.Common;

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

    private string? _connectionString;

    public async Task InitializeAsync()
    {
        await StartLazy.Value;

        var dbName = $"synergyflow_api_test_{Guid.NewGuid():N}";
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

    public AppHttpClient CreateAppHttpClient()
    {
        return new AppHttpClient(CreateManualClient());
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

    private HttpClient CreateManualClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false,
        });
    }
}
