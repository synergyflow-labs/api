namespace SynergyFlow.Domain.Entities.Identity;

public class UserRefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Token { get; init; }

    public required string UserId { get; init; }

    public DateTimeOffset ExpiresOnUtc { get; init; }

    public DateTimeOffset? RevokedOnUtc { get; set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresOnUtc;

    public bool IsActive => RevokedOnUtc == null && !IsExpired;
}
