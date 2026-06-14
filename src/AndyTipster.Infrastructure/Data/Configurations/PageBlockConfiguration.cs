using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class PageBlockConfiguration : IEntityTypeConfiguration<PageBlock>
{
    public void Configure(EntityTypeBuilder<PageBlock> builder)
    {
        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.BlockType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pb => pb.ContentJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(pb => pb.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(pb => new { pb.PageId, pb.SortOrder });
    }
}
