using AndyTipster.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AndyTipster.Infrastructure.Data.Configurations;

public class PlanTransitionPathConfiguration : IEntityTypeConfiguration<PlanTransitionPath>
{
    public void Configure(EntityTypeBuilder<PlanTransitionPath> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TransitionType)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(p => p.SourcePlan)
            .WithMany(plan => plan.UpgradePaths)
            .HasForeignKey(p => p.SourcePlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.TargetPlan)
            .WithMany(plan => plan.DowngradePaths)
            .HasForeignKey(p => p.TargetPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.SourcePlanId, p.TargetPlanId, p.TransitionType })
            .IsUnique();
    }
}
