using SynergyFlow.Domain.Entities.Products;
using SynergyFlow.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace SynergyFlow.Infrastructure.Data;

public sealed class DbInitializer(
    AppDbContext dbContext,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (dbContext.Database.IsRelational())
        {
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(ct);
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync(ct);
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync(ct);
            }

            await EnsureOutboxTriggerAsync(ct);
        }

        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await SeedIdentityUserAsync("admin@synergyflow.local", "Admin123!", "Admin");
        await SeedIdentityUserAsync("user@synergyflow.local", "User123!", "User");

        if (!await dbContext.Products.AnyAsync(ct))
        {
            var keyboardResult = Product.Create(
                name: "Mechanical Keyboard",
                sku: "TECH-KB-001",
                price: 129.99m,
                stockQuantity: 50,
                description: "High-performance mechanical keyboard with tactile switches.");

            var mouseResult = Product.Create(
                name: "Wireless Mouse",
                sku: "TECH-MS-002",
                price: 79.99m,
                stockQuantity: 100,
                description: "Ergonomic wireless gaming mouse.");

            var hubResult = Product.Create(
                name: "USB-C Monitor Hub",
                sku: "TECH-HB-003",
                price: 49.99m,
                stockQuantity: 25,
                description: "10-in-1 multi-port adapter.");

            if (keyboardResult.IsSuccess && mouseResult.IsSuccess && hubResult.IsSuccess)
            {
                await dbContext.Products.AddRangeAsync(
                    [keyboardResult.Value, mouseResult.Value, hubResult.Value],
                    ct);
                await dbContext.SaveChangesAsync(ct);
            }
        }
    }

    private async Task SeedIdentityUserAsync(string email, string password, string role)
    {
        var appUser = await userManager.FindByEmailAsync(email);
        if (appUser is null)
        {
            appUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = role == "Admin" ? "System Administrator" : "Standard User",
            };

            var createResult = await userManager.CreateAsync(appUser, password);
            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(appUser, role);
            }
        }
    }

    private async Task EnsureOutboxTriggerAsync(CancellationToken ct)
    {
        try
        {
            await using var connection = new NpgsqlConnection(dbContext.Database.GetConnectionString());
            await connection.OpenAsync(ct);

            await using var createFunction = connection.CreateCommand();
            createFunction.CommandText = """
                                         CREATE OR REPLACE FUNCTION notify_outbox_insert()
                                         RETURNS trigger
                                         LANGUAGE plpgsql
                                         AS $$
                                         BEGIN
                                             PERFORM pg_notify('outbox_channel', NEW."Id"::text);
                                             RETURN NEW;
                                         END;
                                         $$;
                                         """;
            await createFunction.ExecuteNonQueryAsync(ct);

            await using var dropTrigger = connection.CreateCommand();
            dropTrigger.CommandText = """
                                      DROP TRIGGER IF EXISTS outbox_insert_trigger ON "OutboxMessages";
                                      """;
            await dropTrigger.ExecuteNonQueryAsync(ct);

            await using var createTrigger = connection.CreateCommand();
            createTrigger.CommandText = """
                                        CREATE TRIGGER outbox_insert_trigger
                                            AFTER INSERT ON "OutboxMessages"
                                            FOR EACH ROW
                                            EXECUTE FUNCTION notify_outbox_insert();
                                        """;
            await createTrigger.ExecuteNonQueryAsync(ct);
        }
        catch
        {
            // In unit/integration tests with non-Postgres or limited permissions, fail silently
        }
    }
}
