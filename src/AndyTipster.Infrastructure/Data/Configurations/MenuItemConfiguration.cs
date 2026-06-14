using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(mi => mi.Id);

        builder.Property(mi => mi.Label)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(mi => mi.Url)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(mi => mi.Icon)
            .HasMaxLength(100);

        builder.Property(mi => mi.Target)
            .HasMaxLength(20);

        builder.Property(mi => mi.RequiredRole)
            .HasMaxLength(50);

        builder.Property(mi => mi.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(mi => mi.ParentItem)
            .WithMany(mi => mi.Children)
            .HasForeignKey(mi => mi.ParentItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(mi => new { mi.MenuId, mi.SortOrder });
    }
}
