using System.Text;

using SynergyFlow.Application.Common.Caching;
using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Infrastructure.BackgroundJobs;
using SynergyFlow.Infrastructure.Caching;
using SynergyFlow.Infrastructure.Data;
using SynergyFlow.Infrastructure.Identity;
using SynergyFlow.Infrastructure.Settings;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using Npgsql;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigureSettings(configuration);
        services.AddJwtAuthentication();
        services.ConfigureDataBase(configuration);
        services.ConfigureCaching(configuration);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<DbInitializer>();
        services.AddHostedService<OutboxProcessorJob>();

        return services;
    }

    public static async Task InitializeDevelopmentDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await dbInitializer.InitializeAsync(ct);
    }

    private static IServiceCollection ConfigureCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetPostgresConnectionString(configuration);
        var cacheSettings = configuration.GetSection(CacheSettings.SectionName).Get<CacheSettings>()
            ?? throw new InvalidOperationException("CacheSettings section is not configured.");

        services.AddDistributedPostgreSqlCache(options =>
        {
            options.ConnectionString = connectionString;
            options.SchemaName = cacheSettings.DistributedCacheSchemaName;
            options.TableName = cacheSettings.DistributedCacheTableName;
            options.CreateInfrastructure = true;
        });

        services.AddHybridCache();

        services.AddScoped<ICacheInvalidator, CacheInvalidator>();

        return services;
    }

    private static IServiceCollection ConfigureDataBase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetPostgresConnectionString(configuration);

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(DbInitializer).Assembly.FullName))
            .ConfigureWarnings(warnings =>
                warnings.Ignore(EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddOptions<IdentityOptions>()
            .Configure<IOptions<IdentitySettings>>((options, identitySettings) =>
            {
                options.Password.RequireDigit = identitySettings.Value.RequireDigit;
                options.Password.RequireLowercase = identitySettings.Value.RequireLowercase;
                options.Password.RequireNonAlphanumeric = identitySettings.Value.RequireNonAlphanumeric;
                options.Password.RequireUppercase = identitySettings.Value.RequireUppercase;
                options.Password.RequiredLength = identitySettings.Value.RequiredLength;

                options.User.RequireUniqueEmail = identitySettings.Value.RequireUniqueEmail;
            });

        services.AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services;
    }

    private static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CorsSettings>()
            .Bind(configuration.GetSection(CorsSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<PostgresSettings>()
            .Bind(configuration.GetSection(PostgresSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection(JwtSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RateLimiterSettings>()
            .Bind(configuration.GetSection(RateLimiterSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CacheSettings>()
            .Bind(configuration.GetSection(CacheSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<IdentitySettings>()
            .Bind(configuration.GetSection(IdentitySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static string GetPostgresConnectionString(IConfiguration configuration)
    {
        var postgresSettings = configuration.GetSection(PostgresSettings.SectionName).Get<PostgresSettings>();

        if (postgresSettings is null || string.IsNullOrWhiteSpace(postgresSettings.DefaultConnection))
        {
            throw new InvalidOperationException("The connection string 'DefaultConnection' in 'PostgresSettings' is not configured.");
        }

        var connectionString = postgresSettings.DefaultConnection;
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            MaxPoolSize = postgresSettings.MaximumPoolSize,
            MinPoolSize = postgresSettings.MinimumPoolSize,
            Timeout = postgresSettings.ConnectionTimeoutSeconds,
        };

        return builder.ConnectionString;
    }

    private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Value.Issuer,
                    ValidAudiences = jwtSettings.Value.Audiences,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret)),
                };
            });

        return services;
    }
}
