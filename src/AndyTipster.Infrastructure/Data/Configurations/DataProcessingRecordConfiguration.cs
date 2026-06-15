using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class DataProcessingRecordConfiguration : IEntityTypeConfiguration<DataProcessingRecord>
{
    public void Configure(EntityTypeBuilder<DataProcessingRecord> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.ProcessingType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.Purpose)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.LegalBasis)
            .HasMaxLength(100);

        builder.Property(d => d.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
