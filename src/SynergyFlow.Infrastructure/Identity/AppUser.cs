using SynergyFlow.Domain.Entities.Identity;

using Microsoft.AspNetCore.Identity;

namespace SynergyFlow.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }

    public List<UserRefreshToken> RefreshTokens { get; set; } = [];
}
