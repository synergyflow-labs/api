using System.ComponentModel.DataAnnotations;

namespace SynergyFlow.Application.Common.Caching;

public sealed class CacheSettings
{
    public const string SectionName = "CacheSettings";

    [Range(1, 86_400)]
    public required int OutputCacheDurationSeconds { get; init; }

    [Range(1, 1440)]
    public required int QueryCacheDurationMinutes { get; init; }

    [Required]
    public required string DistributedCacheSchemaName { get; init; }

    [Required]
    public required string DistributedCacheTableName { get; init; }
}
