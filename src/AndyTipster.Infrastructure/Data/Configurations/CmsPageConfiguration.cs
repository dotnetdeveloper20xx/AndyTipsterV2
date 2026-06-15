using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class CmsPageConfiguration : IEntityTypeConfiguration<CmsPage>
{
    public void Configure(EntityTypeBuilder<CmsPage> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(200);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.OgImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.CanonicalUrl)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.BlocksJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Versions)
            .WithOne(v => v.Page)
            .HasForeignKey(v => v.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Blocks)
            .WithOne(b => b.Page)
            .HasForeignKey(b => b.PageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
