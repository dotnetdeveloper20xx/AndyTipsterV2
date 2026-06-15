using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class MediaTagConfiguration : IEntityTypeConfiguration<MediaTag>
{
    public void Configure(EntityTypeBuilder<MediaTag> builder)
    {
        builder.HasKey(mt => mt.Id);

        builder.Property(mt => mt.Tag)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(mt => mt.MediaAsset)
            .WithMany()
            .HasForeignKey(mt => mt.MediaAssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mt => new { mt.MediaAssetId, mt.Tag })
            .IsUnique();
    }
}
