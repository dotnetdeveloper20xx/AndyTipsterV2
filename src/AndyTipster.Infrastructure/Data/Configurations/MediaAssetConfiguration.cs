using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FileName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(m => m.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.BlobUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(m => m.CdnUrl)
            .HasMaxLength(1000);

        builder.Property(m => m.AltText)
            .HasMaxLength(500);

        builder.Property(m => m.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(m => m.Folder)
            .WithMany(f => f.Assets)
            .HasForeignKey(m => m.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.UploadedByUser)
            .WithMany()
            .HasForeignKey(m => m.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
