using SynergyFlow.Api.Endpoints;
using SynergyFlow.Infrastructure.Data;
using SynergyFlow.Infrastructure.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Scalar.AspNetCore;

using Serilog;

Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseResponseCompression();
app.UseExceptionHandler();
app.UseOutputCache();

var corsSettings = app.Services.GetRequiredService<IOptions<CorsSettings>>().Value;
app.UseCors(corsSettings.PolicyName);

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Configuration.GetValue<bool>("ResetDatabase"))
{
    using var scope = app.Services.CreateScope();
    var resetCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await resetCtx.Database.ExecuteSqlRawAsync("DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public;");
}

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("AutoMigrateDb"))
{
    await app.Services.InitializeDevelopmentDatabaseAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Clean Architecture API V1");
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
        options.EnableFilter();
    });

    app.MapScalarApiReference();
}
else
{
    app.UseHsts();
}

app.MapHealthChecks("/healthz", new() { AllowCachingResponses = false });
app.MapHealthChecks("/healthz/ready", new()
{
    Predicate = reg => reg.Tags.Contains("ready"),
    AllowCachingResponses = false,
});
app.MapHealthChecks("/healthz/startup", new()
{
    Predicate = reg => reg.Tags.Contains("ready"),
    AllowCachingResponses = false,
});

app.MapControllers();
app.MapAllEndpoints();

app.Run();

// Assembly marker for WebApplicationFactory<Program> in Integration Tests
public partial class Program;
