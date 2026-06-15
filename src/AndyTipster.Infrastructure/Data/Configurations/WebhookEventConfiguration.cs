using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class WebhookEventConfiguration : IEntityTypeConfiguration<WebhookEvent>
{
    public void Configure(EntityTypeBuilder<WebhookEvent> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.ExternalEventId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(w => w.Provider)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(w => w.EventType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(w => w.ReceivedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(w => w.ExternalEventId)
            .IsUnique();
    }
}
