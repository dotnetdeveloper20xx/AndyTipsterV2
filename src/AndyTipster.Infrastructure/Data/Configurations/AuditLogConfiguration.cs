using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActionType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.TargetEntity)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.TargetEntityId)
            .HasMaxLength(100);

        builder.Property(a => a.BeforeJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.AfterJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45);

        builder.HasIndex(a => a.ActorUserId);
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => new { a.TargetEntity, a.TargetEntityId });
    }
}
