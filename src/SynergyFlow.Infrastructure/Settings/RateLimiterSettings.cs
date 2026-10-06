using System.ComponentModel.DataAnnotations;

namespace SynergyFlow.Infrastructure.Settings;

public sealed class RateLimiterSettings
{
    public const string SectionName = "RateLimiterSettings";

    public bool DisableRateLimiting { get; init; }

    [Range(1, 1000)]
    public required int GlobalConcurrencyPermitLimit { get; init; }

    [Range(0, 10_000)]
    public required int GlobalConcurrencyQueueLimit { get; init; }

    [Range(1, 1440)]
    public required int AuthWindowMinutes { get; init; }

    [Range(1, 1000)]
    public required int AuthPermitLimit { get; init; }

    [Range(1, 100)]
    public required int AuthSegmentsPerWindow { get; init; }
}
