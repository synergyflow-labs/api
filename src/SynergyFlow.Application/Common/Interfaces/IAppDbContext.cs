using SynergyFlow.Domain.Common;
using SynergyFlow.Domain.Entities.Identity;
using SynergyFlow.Domain.Entities.Products;

using Microsoft.EntityFrameworkCore;

namespace SynergyFlow.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Product> Products { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }

    DbSet<UserRefreshToken> UserRefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
