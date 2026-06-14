using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class GdprConsentConfiguration : IEntityTypeConfiguration<GdprConsent>
{
    public void Configure(EntityTypeBuilder<GdprConsent> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.ConsentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(g => g.IpAddress)
            .HasMaxLength(45);

        builder.Property(g => g.UserAgent)
            .HasMaxLength(500);

        builder.Property(g => g.GrantedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(g => new { g.UserId, g.ConsentType });
    }
}
