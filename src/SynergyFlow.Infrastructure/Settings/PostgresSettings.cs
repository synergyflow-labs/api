using System.ComponentModel.DataAnnotations;

namespace SynergyFlow.Infrastructure.Settings;

public sealed class PostgresSettings
{
    public const string SectionName = "PostgresSettings";

    [Required]
    public required string DefaultConnection { get; init; }

    [Required]
    public required int MaximumPoolSize { get; init; }

    [Required]
    public required int MinimumPoolSize { get; init; }

    [Required]
    public required int ConnectionTimeoutSeconds { get; init; }
}
