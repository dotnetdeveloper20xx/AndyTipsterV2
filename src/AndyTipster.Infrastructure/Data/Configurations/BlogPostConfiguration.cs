using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.HasKey(bp => bp.Id);

        builder.Property(bp => bp.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(bp => bp.Slug)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(bp => bp.Content)
            .IsRequired();

        builder.Property(bp => bp.Excerpt)
            .HasMaxLength(500);

        builder.Property(bp => bp.FeaturedImageUrl)
            .HasMaxLength(500);

        builder.Property(bp => bp.MetaTitle)
            .HasMaxLength(200);

        builder.Property(bp => bp.MetaDescription)
            .HasMaxLength(500);

        builder.Property(bp => bp.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(bp => bp.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(bp => bp.Author)
            .WithMany()
            .HasForeignKey(bp => bp.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(bp => bp.Slug)
            .IsUnique();
    }
}
