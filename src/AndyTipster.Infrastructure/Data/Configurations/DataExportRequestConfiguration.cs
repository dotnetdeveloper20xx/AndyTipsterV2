using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class DataExportRequestConfiguration : IEntityTypeConfiguration<DataExportRequest>
{
    public void Configure(EntityTypeBuilder<DataExportRequest> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Format)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(d => d.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.DownloadUrl)
            .HasMaxLength(1000);

        builder.Property(d => d.RequestedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
