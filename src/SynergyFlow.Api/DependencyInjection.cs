using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

using Asp.Versioning;

using SynergyFlow.Api.Infrastructure;
using SynergyFlow.Api.OpenApi.Transformers;
using SynergyFlow.Api.Services;
using SynergyFlow.Application.Common.Caching;
using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Infrastructure.Settings;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

using Npgsql;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllerWithJsonConfiguration();
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
        services.AddCustomVersioning();
        services.AddApiDocumentation();
        services.AddAppOpenTelemetry();
        services.AddGracefulShutdown();

        services.AddOutputCache();
        services.AddOptions<OutputCacheOptions>()
            .Configure<IOptions<CacheSettings>>((options, cacheSettings) =>
            {
                options.AddBasePolicy(builder =>
                    builder.Expire(TimeSpan.FromSeconds(cacheSettings.Value.OutputCacheDurationSeconds)));
            });

        services.AddRateLimiter();
        services.AddOptions<RateLimiterOptions>()
            .Configure<IOptions<RateLimiterSettings>>((options, rateLimiterSettings) =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                var settings = rateLimiterSettings.Value;

                if (settings.DisableRateLimiting)
                {
                    options.AddPolicy(RateLimiterPolicies.Global, _ => RateLimitPartition.GetNoLimiter("global"));
                    options.AddPolicy(RateLimiterPolicies.Auth, _ => RateLimitPartition.GetNoLimiter("auth"));
                    return;
                }

                options.AddConcurrencyLimiter(RateLimiterPolicies.Global, limiter =>
                {
                    limiter.PermitLimit = settings.GlobalConcurrencyPermitLimit;
                    limiter.QueueLimit = settings.GlobalConcurrencyQueueLimit;
                    limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });

                options.AddPolicy(RateLimiterPolicies.Auth, ctx =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new SlidingWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(settings.AuthWindowMinutes),
                            PermitLimit = settings.AuthPermitLimit,
                            SegmentsPerWindow = settings.AuthSegmentsPerWindow,
                            QueueLimit = 0,
                        }));
            });

        services.AddConfiguredCors();
        services.AddExceptionHandling();
        services.AddCustomProblemDetails();
        services.AddAuthorization();
        services.AddCustomResponseCompression();
        services.AddIdentityInfrastructure();

        return services;
    }

    private static IServiceCollection AddGracefulShutdown(this IServiceCollection services)
    {
        services.Configure<HostOptions>(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(30);
        });
        return services;
    }

    private static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();
        return services;
    }

    private static IServiceCollection AddCustomResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        return services;
    }

    private static IServiceCollection AddCustomVersioning(this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        return services;
    }

    private static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        string[] versions = ["v1"];

        foreach (var version in versions)
        {
            services.AddOpenApi(
                version,
                options =>
                {
                    options.AddDocumentTransformer<VersionInfoTransformer>();
                    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                    options.AddOperationTransformer<BearerSecurityOperationTransformer>();
                });
        }

        return services;
    }

    private static IServiceCollection AddControllerWithJsonConfiguration(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.AllowOutOfOrderMetadataProperties = true;
        });

        return services;
    }

    private static IServiceCollection AddConfiguredCors(this IServiceCollection services)
    {
        services.AddCors();
        services.AddOptions<CorsOptions>()
            .Configure<IOptions<CorsSettings>>((options, corsSettings) =>
            {
                options.AddPolicy(
                    corsSettings.Value.PolicyName,
                    policy =>
                    {
                        policy.WithOrigins(corsSettings.Value.AllowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
            });

        return services;
    }

    private static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    private static IServiceCollection AddCustomProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance =
                $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
        });
        return services;
    }

    private static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(res => res.AddService("synergyflow-api"))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
                tracing.AddNpgsql();
                tracing.AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation();
                metrics.AddOtlpExporter();
            });

        return services;
    }
}
