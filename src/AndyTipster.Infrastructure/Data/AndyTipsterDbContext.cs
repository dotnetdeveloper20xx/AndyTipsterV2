using AndyTipster.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Data;

public class AndyTipsterDbContext : IdentityDbContext<ApplicationUser, Role, Guid, 
    Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>,
    UserRole,
    Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>,
    Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>,
    Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>
{
    public AndyTipsterDbContext(DbContextOptions<AndyTipsterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Tip> Tips => Set<Tip>();
    public DbSet<TipCategory> TipCategories => Set<TipCategory>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CmsPage> CmsPages => Set<CmsPage>();
    public DbSet<PageVersion> PageVersions => Set<PageVersion>();
    public DbSet<PageBlock> PageBlocks => Set<PageBlock>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<MediaFolder> MediaFolders => Set<MediaFolder>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<GdprConsent> GdprConsents => Set<GdprConsent>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<NavigationMenu> NavigationMenus => Set<NavigationMenu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AndyTipsterDbContext).Assembly);
    }
}
