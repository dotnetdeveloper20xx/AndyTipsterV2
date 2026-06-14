using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReferralCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.ReferredEmail)
            .HasMaxLength(256);

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(r => r.ReferralCode)
            .IsUnique();

        builder.HasOne(r => r.ReferredUser)
            .WithMany()
            .HasForeignKey(r => r.ReferredUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
