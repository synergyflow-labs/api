using System.ComponentModel.DataAnnotations;

namespace SynergyFlow.Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required]
    public required string Issuer { get; init; }

    public required string[] Audiences { get; init; } = [];

    [Required]
    [MinLength(32)]
    public required string Secret { get; init; }

    [Range(1, 1440)]
    public int ExpiryMinutes { get; init; }

    [Range(1, 365)]
    public required int RefreshTokenExpirationDays { get; init; }
}
