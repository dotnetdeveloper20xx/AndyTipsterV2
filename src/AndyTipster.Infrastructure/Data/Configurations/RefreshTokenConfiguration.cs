using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(500);

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(rt => rt.Token);

        builder.Ignore(rt => rt.IsRevoked);
        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsActive);
    }
}
