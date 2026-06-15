using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class SiteSettingsConfiguration : IEntityTypeConfiguration<SiteSettings>
{
    public void Configure(EntityTypeBuilder<SiteSettings> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.SiteName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Tagline)
            .HasMaxLength(250);

        builder.Property(s => s.LogoLightUrl)
            .HasMaxLength(500);

        builder.Property(s => s.LogoDarkUrl)
            .HasMaxLength(500);

        builder.Property(s => s.FaviconUrl)
            .HasMaxLength(500);

        builder.Property(s => s.MaintenanceMessage)
            .HasMaxLength(1000);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
