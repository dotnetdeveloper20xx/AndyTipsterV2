using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class TipConfiguration : IEntityTypeConfiguration<Tip>
{
    public void Configure(EntityTypeBuilder<Tip> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.RaceName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Selection)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Odds)
            .HasPrecision(8, 2);

        builder.Property(t => t.Commentary)
            .HasMaxLength(5000);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.Result)
            .HasConversion<string?>()
            .HasMaxLength(20);

        builder.Property(t => t.ProfitLoss)
            .HasPrecision(10, 2);

        builder.Property(t => t.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(t => t.Category)
            .WithMany(tc => tc.Tips)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreatedByUser)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Tip)
            .HasForeignKey(c => c.TipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.EventDate);
        builder.HasIndex(t => t.Status);
    }
}
