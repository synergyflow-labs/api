using SynergyFlow.Application.Common.Interfaces;
using SynergyFlow.Infrastructure.Identity;

namespace SynergyFlow.Tests.Common.Security;

public class TestCurrentUser : IUser
{
    private static readonly AsyncLocal<AppUser?> CurrentUserHolder = new();

    public string? Id => CurrentUserHolder.Value?.Id;

    public string? Email => CurrentUserHolder.Value?.Email;

    public string? Role => "Admin";

    public static void Set(AppUser? currentUser)
    {
        CurrentUserHolder.Value = currentUser;
    }
}
