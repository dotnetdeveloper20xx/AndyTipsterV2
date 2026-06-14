using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(pc => pc.DiscountType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(pc => pc.DiscountValue)
            .HasPrecision(10, 2);

        builder.Property(pc => pc.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(pc => pc.Code)
            .IsUnique();

        builder.HasMany(pc => pc.ApplicablePlans)
            .WithMany()
            .UsingEntity("PromoCodePlan");
    }
}
