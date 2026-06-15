using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class NavigationMenuConfiguration : IEntityTypeConfiguration<NavigationMenu>
{
    public void Configure(EntityTypeBuilder<NavigationMenu> builder)
    {
        builder.HasKey(nm => nm.Id);

        builder.Property(nm => nm.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(nm => nm.Location)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(nm => nm.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(nm => nm.Items)
            .WithOne(mi => mi.Menu)
            .HasForeignKey(mi => mi.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
