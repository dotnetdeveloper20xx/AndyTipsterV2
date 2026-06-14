using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class PageVersionConfiguration : IEntityTypeConfiguration<PageVersion>
{
    public void Configure(EntityTypeBuilder<PageVersion> builder)
    {
        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.BlocksJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(pv => pv.ChangeSummary)
            .HasMaxLength(500);

        builder.Property(pv => pv.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(pv => pv.Author)
            .WithMany()
            .HasForeignKey(pv => pv.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pv => new { pv.PageId, pv.VersionNumber })
            .IsUnique();
    }
}
