using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ExternalSubscriptionId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Provider)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(s => s.ExternalSubscriptionId);

        builder.HasMany(s => s.Payments)
            .WithOne(p => p.Subscription)
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
