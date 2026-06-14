using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(p => p.Price)
            .HasPrecision(10, 2);

        builder.Property(p => p.SetupFee)
            .HasPrecision(10, 2);

        builder.Property(p => p.Currency)
            .HasConversion<string>()
            .HasMaxLength(3);

        builder.Property(p => p.BillingCycle)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.SyncStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.PayPalPlanId)
            .HasMaxLength(50);

        builder.Property(p => p.StripePriceId)
            .HasMaxLength(50);

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.HasMany(p => p.Subscriptions)
            .WithOne(s => s.Plan)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.IncludedCategories)
            .WithMany(tc => tc.Plans)
            .UsingEntity("PlanTipCategory");
    }
}
