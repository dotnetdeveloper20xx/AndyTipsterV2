using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(10, 2);

        builder.Property(p => p.Fees)
            .HasPrecision(10, 2);

        builder.Property(p => p.Net)
            .HasPrecision(10, 2);

        builder.Property(p => p.Currency)
            .HasConversion<string>()
            .HasMaxLength(3);

        builder.Property(p => p.Provider)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.ExternalTransactionId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(p => p.ExternalTransactionId);
    }
}
