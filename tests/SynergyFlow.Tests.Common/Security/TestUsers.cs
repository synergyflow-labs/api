using SynergyFlow.Infrastructure.Identity;

namespace SynergyFlow.Tests.Common.Security;

public class TestUserAccount
{
    public required AppUser User { get; init; }

    public required string Password { get; init; }
}

public static class TestUsers
{
    public static TestUserAccount Admin => new()
    {
        User = new AppUser
        {
            Id = "d7d11db8-0ce0-48b4-8ab3-7729cb4187f5",
            Email = "admin@synergyflow.local",
            UserName = "admin@synergyflow.local",
            EmailConfirmed = true,
            FullName = "System Administrator",
        },
        Password = "Admin123!",
    };

    public static TestUserAccount User => new()
    {
        User = new AppUser
        {
            Id = "54cd01ba-b9ae-4c14-bab6-f3df0219ba4c",
            Email = "user@synergyflow.local",
            UserName = "user@synergyflow.local",
            EmailConfirmed = true,
            FullName = "Standard User",
        },
        Password = "User123!",
    };
}
