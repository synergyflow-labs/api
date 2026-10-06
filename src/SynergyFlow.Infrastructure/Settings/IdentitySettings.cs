using System.ComponentModel.DataAnnotations;

namespace SynergyFlow.Infrastructure.Settings;

public sealed class IdentitySettings
{
    public const string SectionName = "IdentitySettings";

    public required bool RequireDigit { get; init; }

    public required bool RequireLowercase { get; init; }

    public required bool RequireNonAlphanumeric { get; init; }

    public required bool RequireUppercase { get; init; }

    [Range(1, 128)]
    public required int RequiredLength { get; init; }

    public required bool RequireUniqueEmail { get; init; }
}
