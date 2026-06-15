using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds comprehensive demo data for development environments.
/// All operations are idempotent — existing records are not duplicated.
/// </summary>
public static class DemoSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AndyTipsterDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AndyTipsterDbContext>>();

        var adminUser = await SeedUsersAsync(userManager, logger);
        await SeedPlansAsync(dbContext, logger);
        var categories = await SeedTipCategoriesAsync(dbContext, logger);
        await SeedTipsAsync(dbContext, adminUser, categories, logger);
        await SeedBlogPostsAsync(dbContext, adminUser, logger);
        await SeedSiteSettingsAsync(dbContext, logger);
        await SeedNavigationMenuAsync(dbContext, logger);
    }

    private static async Task<ApplicationUser> SeedUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        var users = new[]
        {
            new { Email = "admin@andytipster.com", Password = "Admin123!", DisplayName = "Andy Admin", Role = "Super Admin" },
            new { Email = "subscriber@test.com", Password = "Test123!", DisplayName = "John Subscriber", Role = "Subscriber" },
            new { Email = "free@test.com", Password = "Test123!", DisplayName = "Jane Free", Role = "Free User" },
        };

        ApplicationUser? adminUser = null;

        foreach (var userData in users)
        {
            var existing = await userManager.FindByEmailAsync(userData.Email);
            if (existing is not null)
            {
                if (userData.Role == "Super Admin") adminUser = existing;
                continue;
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = userData.Email,
                UserName = userData.Email,
                NormalizedEmail = userData.Email.ToUpperInvariant(),
                NormalizedUserName = userData.Email.ToUpperInvariant(),
                DisplayName = userData.DisplayName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
            };

            var result = await userManager.CreateAsync(user, userData.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, userData.Role);
                logger.LogInformation("Seeded user: {Email} with role {Role}", userData.Email, userData.Role);

                if (userData.Role == "Super Admin") adminUser = user;
            }
            else
            {
                logger.LogWarning("Failed to seed user {Email}: {Errors}", userData.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        return adminUser ?? throw new InvalidOperationException("Admin user could not be seeded or found.");
    }

    private static async Task SeedPlansAsync(AndyTipsterDbContext dbContext, ILogger logger)
    {
        var plans = new[]
        {
            new Plan
            {
                Id = Guid.NewGuid(),
                Name = "Monthly Premium",
                Slug = "monthly-premium",
                Price = 19.99m,
                Currency = Currency.GBP,
                BillingCycle = BillingCycle.Monthly,
                TrialPeriodDays = 0,
                AutoRenew = true,
                IsActive = true,
                SyncStatus = PlanSyncStatus.SyncPending,
                CreatedAt = DateTime.UtcNow,
            },
            new Plan
            {
                Id = Guid.NewGuid(),
                Name = "Quarterly Value",
                Slug = "quarterly-value",
                Price = 49.99m,
                Currency = Currency.GBP,
                BillingCycle = BillingCycle.Quarterly,
                TrialPeriodDays = 0,
                AutoRenew = true,
                IsActive = true,
                SyncStatus = PlanSyncStatus.SyncPending,
                CreatedAt = DateTime.UtcNow,
            },
            new Plan
            {
                Id = Guid.NewGuid(),
                Name = "Annual Gold",
                Slug = "annual-gold",
                Price = 149.99m,
                Currency = Currency.GBP,
                BillingCycle = BillingCycle.Annual,
                TrialPeriodDays = 7,
                AutoRenew = true,
                IsActive = true,
                SyncStatus = PlanSyncStatus.SyncPending,
                CreatedAt = DateTime.UtcNow,
            },
        };

        foreach (var plan in plans)
        {
            var exists = await dbContext.Plans.AnyAsync(p => p.Name == plan.Name);
            if (!exists)
            {
                dbContext.Plans.Add(plan);
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Subscription plans seeded successfully.");
    }

    private static async Task<List<TipCategory>> SeedTipCategoriesAsync(AndyTipsterDbContext dbContext, ILogger logger)
    {
        var categoryDefs = new[]
        {
            new { Name = "UK Horse Racing", Slug = "uk-horse-racing", Description = "Tips for UK horse racing events" },
            new { Name = "Irish Horse Racing", Slug = "irish-horse-racing", Description = "Tips for Irish horse racing events" },
            new { Name = "Other Sports", Slug = "other-sports", Description = "Tips for other sporting events" },
        };

        foreach (var def in categoryDefs)
        {
            var exists = await dbContext.TipCategories.AnyAsync(c => c.Name == def.Name);
            if (!exists)
            {
                dbContext.TipCategories.Add(new TipCategory
                {
                    Id = Guid.NewGuid(),
                    Name = def.Name,
                    Slug = def.Slug,
                    Description = def.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                });
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Tip categories seeded successfully.");

        return await dbContext.TipCategories.ToListAsync();
    }

    private static async Task SeedTipsAsync(AndyTipsterDbContext dbContext, ApplicationUser adminUser, List<TipCategory> categories, ILogger logger)
    {
        if (await dbContext.Tips.AnyAsync()) return;

        var ukCategory = categories.First(c => c.Name == "UK Horse Racing");
        var irishCategory = categories.First(c => c.Name == "Irish Horse Racing");
        var otherCategory = categories.First(c => c.Name == "Other Sports");

        var tips = new List<Tip>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-10),
                RaceName = "Cheltenham Gold Cup",
                Selection = "Golden Arrow",
                Odds = 4.5m,
                Stake = 2,
                CategoryId = ukCategory.Id,
                Commentary = "Strong form, good ground conditions.",
                Status = TipStatus.Published,
                Result = TipResult.Won,
                ProfitLoss = 7.0m,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                PublishedAt = DateTime.UtcNow.AddDays(-10),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-9),
                RaceName = "Ascot Stakes",
                Selection = "Blue Thunder",
                Odds = 3.0m,
                Stake = 1,
                CategoryId = ukCategory.Id,
                Commentary = "Consistent performer at this distance.",
                Status = TipStatus.Published,
                Result = TipResult.Lost,
                ProfitLoss = -1.0m,
                CreatedAt = DateTime.UtcNow.AddDays(-9),
                PublishedAt = DateTime.UtcNow.AddDays(-9),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-8),
                RaceName = "Leopardstown Chase",
                Selection = "Irish Dancer",
                Odds = 5.0m,
                Stake = 1,
                CategoryId = irishCategory.Id,
                Commentary = "Excellent record at Leopardstown.",
                Status = TipStatus.Published,
                Result = TipResult.Won,
                ProfitLoss = 4.0m,
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                PublishedAt = DateTime.UtcNow.AddDays(-8),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-7),
                RaceName = "Curragh Derby Trial",
                Selection = "Emerald Star",
                Odds = 2.5m,
                Stake = 2,
                CategoryId = irishCategory.Id,
                Commentary = "Top jockey booked.",
                Status = TipStatus.Published,
                Result = TipResult.Void,
                ProfitLoss = 0m,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                PublishedAt = DateTime.UtcNow.AddDays(-7),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-6),
                RaceName = "Premier League Match",
                Selection = "Man City to Win",
                Odds = 1.8m,
                Stake = 2,
                CategoryId = otherCategory.Id,
                Commentary = "Home advantage and strong recent form.",
                Status = TipStatus.Published,
                Result = TipResult.Won,
                ProfitLoss = 1.6m,
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                PublishedAt = DateTime.UtcNow.AddDays(-6),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-5),
                RaceName = "York Handicap",
                Selection = "Northern Lad",
                Odds = 6.0m,
                Stake = 1,
                CategoryId = ukCategory.Id,
                Commentary = "Big price but good each-way value.",
                Status = TipStatus.Published,
                Result = TipResult.Lost,
                ProfitLoss = -1.0m,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                PublishedAt = DateTime.UtcNow.AddDays(-5),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-4),
                RaceName = "Galway Hurdle",
                Selection = "Celtic Thunder",
                Odds = 3.5m,
                Stake = 2,
                CategoryId = irishCategory.Id,
                Commentary = "Trainer in excellent form.",
                Status = TipStatus.Published,
                Result = TipResult.Won,
                ProfitLoss = 5.0m,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                PublishedAt = DateTime.UtcNow.AddDays(-4),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-3),
                RaceName = "Champions League Match",
                Selection = "Over 2.5 Goals",
                Odds = 2.0m,
                Stake = 1,
                CategoryId = otherCategory.Id,
                Commentary = "Both teams score freely.",
                Status = TipStatus.Published,
                Result = TipResult.Lost,
                ProfitLoss = -1.0m,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                PublishedAt = DateTime.UtcNow.AddDays(-3),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(-2),
                RaceName = "Newmarket 2000 Guineas",
                Selection = "Speed Demon",
                Odds = 4.0m,
                Stake = 2,
                CategoryId = ukCategory.Id,
                Commentary = "Impressive trial form.",
                Status = TipStatus.Published,
                Result = TipResult.Won,
                ProfitLoss = 6.0m,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                PublishedAt = DateTime.UtcNow.AddDays(-2),
                CreatedByUserId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                EventDate = DateTime.UtcNow.AddDays(1),
                RaceName = "Epsom Derby",
                Selection = "Royal Ambition",
                Odds = 7.0m,
                Stake = 1,
                CategoryId = ukCategory.Id,
                Commentary = "Each-way selection with a live chance.",
                Status = TipStatus.Published,
                Result = null,
                ProfitLoss = null,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
                CreatedByUserId = adminUser.Id,
            },
        };

        dbContext.Tips.AddRange(tips);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Sample tips seeded successfully ({Count} tips).", tips.Count);
    }

    private static async Task SeedBlogPostsAsync(AndyTipsterDbContext dbContext, ApplicationUser adminUser, ILogger logger)
    {
        if (await dbContext.BlogPosts.AnyAsync()) return;

        var posts = new List<BlogPost>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Welcome to AndyTipster",
                Slug = "welcome-to-andytipster",
                Content = "<p>Welcome to AndyTipster! We provide premium horse racing tips backed by years of experience and data analysis. Subscribe today to gain access to our expert selections.</p>",
                Excerpt = "Welcome to AndyTipster — your source for premium horse racing tips.",
                Status = PageStatus.Published,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                PublishedAt = DateTime.UtcNow.AddDays(-5),
                AuthorId = adminUser.Id,
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Cheltenham Festival Preview 2025",
                Slug = "cheltenham-festival-preview-2025",
                Content = "<p>Our comprehensive preview of the Cheltenham Festival is coming soon. Stay tuned for ante-post tips and race-by-race analysis.</p>",
                Excerpt = "Preview of the upcoming Cheltenham Festival with tips and analysis.",
                Status = PageStatus.Draft,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                PublishedAt = null,
                AuthorId = adminUser.Id,
            },
        };

        dbContext.BlogPosts.AddRange(posts);
        await dbContext.SaveChangesAsync();
        logger.LogInformation("Blog posts seeded successfully.");
    }

    private static async Task SeedSiteSettingsAsync(AndyTipsterDbContext dbContext, ILogger logger)
    {
        if (await dbContext.SiteSettings.AnyAsync()) return;

        dbContext.SiteSettings.Add(new SiteSettings
        {
            Id = Guid.NewGuid(),
            SiteName = "AndyTipster",
            Tagline = "Premium Horse Racing Tips",
            MaintenanceMode = false,
            CreatedAt = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Site settings seeded successfully.");
    }

    private static async Task SeedNavigationMenuAsync(AndyTipsterDbContext dbContext, ILogger logger)
    {
        if (await dbContext.NavigationMenus.AnyAsync(m => m.Location == "header")) return;

        var menuId = Guid.NewGuid();
        var menu = new NavigationMenu
        {
            Id = menuId,
            Name = "Main Navigation",
            Location = "header",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        dbContext.NavigationMenus.Add(menu);

        var menuItems = new[]
        {
            new { Label = "Home", Url = "/", Order = 1 },
            new { Label = "Tips", Url = "/tips", Order = 2 },
            new { Label = "Pricing", Url = "/pricing", Order = 3 },
            new { Label = "Blog", Url = "/blog", Order = 4 },
            new { Label = "Contact", Url = "/contact", Order = 5 },
        };

        foreach (var item in menuItems)
        {
            dbContext.MenuItems.Add(new MenuItem
            {
                Id = Guid.NewGuid(),
                MenuId = menuId,
                Label = item.Label,
                Url = item.Url,
                SortOrder = item.Order,
                IsVisible = true,
                CreatedAt = DateTime.UtcNow,
            });
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Navigation menu seeded successfully.");
    }
}
