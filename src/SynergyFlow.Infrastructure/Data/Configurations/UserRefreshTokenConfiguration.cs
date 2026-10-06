using SynergyFlow.Domain.Entities.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SynergyFlow.Infrastructure.Data.Configurations;

public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("UserRefreshTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(r => r.Token);
        builder.HasIndex(r => r.UserId);

        builder.Property(r => r.ExpiresOnUtc)
            .IsRequired();

        builder.Property(r => r.RevokedOnUtc);
    }
}
