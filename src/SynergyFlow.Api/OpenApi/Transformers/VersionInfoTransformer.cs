using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SynergyFlow.Api.OpenApi.Transformers;

internal sealed class VersionInfoTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken ct)
    {
        var version = context.DocumentName;

        document.Info = new OpenApiInfo
        {
            Version = version,
            Title = $"Clean Architecture API ({version})",
            Description = "Enterprise Clean Architecture API template built with .NET 10, EF Core, MediatR, and PostgreSQL Outbox.",
        };

        return Task.CompletedTask;
    }
}
