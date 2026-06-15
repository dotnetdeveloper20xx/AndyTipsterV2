using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class RedirectConfiguration : IEntityTypeConfiguration<Redirect>
{
    public void Configure(EntityTypeBuilder<Redirect> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FromPath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.ToPath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(r => r.FromPath)
            .IsUnique();
    }
}
