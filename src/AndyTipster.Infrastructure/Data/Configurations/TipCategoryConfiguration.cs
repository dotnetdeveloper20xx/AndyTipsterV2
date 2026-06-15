using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class TipCategoryConfiguration : IEntityTypeConfiguration<TipCategory>
{
    public void Configure(EntityTypeBuilder<TipCategory> builder)
    {
        builder.HasKey(tc => tc.Id);

        builder.Property(tc => tc.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(tc => tc.Slug)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(tc => tc.Description)
            .HasMaxLength(500);

        builder.Property(tc => tc.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(tc => tc.Slug)
            .IsUnique();
    }
}
