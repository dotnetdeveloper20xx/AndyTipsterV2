using System.Text;
using AndyTipster.Application.Audit.Services;
using AndyTipster.Application.Auth.Services;
using AndyTipster.Application.Roles.Services;
using AndyTipster.Application.Users.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Authorization;
using AndyTipster.Infrastructure.Configuration;
using AndyTipster.Infrastructure.Data;
using AndyTipster.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AndyTipster.Infrastructure;

/// <summary>
/// Registers Infrastructure layer services (DbContext, repositories, external clients) with the DI container.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AndyTipsterDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(AndyTipsterDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // Configure ASP.NET Core Identity with password requirements, lockout, and token settings
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Password complexity: 8+ chars, uppercase, lowercase, digit, special char
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;

                // Require confirmed email for sign-in
                options.SignIn.RequireConfirmedEmail = true;

                // User settings
                options.User.RequireUniqueEmail = true;

                // Account lockout: 5 failed attempts → 15-minute lock
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                // Token providers - set email confirmation token lifespan to 24 hours
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultProvider;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<AndyTipsterDbContext>()
            .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider)
            .AddTokenProvider<EmailTokenProvider<ApplicationUser>>(TokenOptions.DefaultEmailProvider)
            .AddTokenProvider<AuthenticatorTokenProvider<ApplicationUser>>(TokenOptions.DefaultAuthenticatorProvider);

        // Configure token lifespan to 24 hours for email confirmation tokens
        services.Configure<DataProtectionTokenProviderOptions>(TokenOptions.DefaultEmailProvider, options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24);
        });

        // Configure password reset token lifespan to 1 hour (single-use via Identity's built-in stamp validation)
        services.Configure<DataProtectionTokenProviderOptions>(TokenOptions.DefaultProvider, options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(1);
        });

        // Configure JWT settings
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSettings);

        // Configure JWT Bearer authentication
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = jwtSettings["Key"]
                    ?? throw new InvalidOperationException("JWT Key is not configured.");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero, // No clock skew for precise token expiry
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };

                // Ensure proper 401/403 responses for unauthenticated/unauthorized requests
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        // Return 401 for unauthenticated requests
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";
                        var problem = new
                        {
                            type = "https://andytipster.com/errors/unauthenticated",
                            title = "Unauthorized",
                            status = 401,
                            detail = "Authentication is required to access this resource.",
                            instance = context.Request.Path.Value
                        };
                        return context.Response.WriteAsJsonAsync(problem);
                    },
                    OnForbidden = context =>
                    {
                        // Return 403 for authenticated but unauthorized requests
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/problem+json";
                        var problem = new
                        {
                            type = "https://andytipster.com/errors/forbidden",
                            title = "Forbidden",
                            status = 403,
                            detail = "You do not have permission to access this resource.",
                            instance = context.Request.Path.Value
                        };
                        return context.Response.WriteAsJsonAsync(problem);
                    }
                };
            });

        // Register application services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISocialAuthService, SocialAuthService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        // Register HttpClientFactory for social auth provider calls
        services.AddHttpClient("SocialAuth", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Register permission-based authorization handler
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

        // Register permission-based authorization policies
        services.AddAuthorizationBuilder()
            .AddPolicy("Permission:Users.View", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.UsersView)))
            .AddPolicy("Permission:Users.Create", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.UsersCreate)))
            .AddPolicy("Permission:Users.Edit", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.UsersEdit)))
            .AddPolicy("Permission:Users.Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.UsersDelete)))
            .AddPolicy("Permission:Users.Impersonate", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.UsersImpersonate)))
            .AddPolicy("Permission:Roles.View", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.RolesView)))
            .AddPolicy("Permission:Roles.Create", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.RolesCreate)))
            .AddPolicy("Permission:Roles.Edit", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.RolesEdit)))
            .AddPolicy("Permission:Roles.Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.RolesDelete)))
            .AddPolicy("Permission:Roles.Assign", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.RolesAssign)))
            .AddPolicy("Permission:Plans.View", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.PlansView)))
            .AddPolicy("Permission:Plans.Create", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.PlansCreate)))
            .AddPolicy("Permission:Plans.Edit", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.PlansEdit)))
            .AddPolicy("Permission:Plans.Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.PlansDelete)))
            .AddPolicy("Permission:Tips.View", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.TipsView)))
            .AddPolicy("Permission:Tips.Create", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.TipsCreate)))
            .AddPolicy("Permission:Tips.Edit", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.TipsEdit)))
            .AddPolicy("Permission:Tips.Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.TipsDelete)))
            .AddPolicy("Permission:CMS.View", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CmsView)))
            .AddPolicy("Permission:CMS.Create", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CmsCreate)))
            .AddPolicy("Permission:CMS.Edit", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CmsEdit)))
            .AddPolicy("Permission:CMS.Delete", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CmsDelete)))
            .AddPolicy("Permission:CMS.Publish", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.CmsPublish)))
            .AddPolicy("Permission:Analytics.View", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AnalyticsView)))
            .AddPolicy("Permission:Subscriptions.View", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.SubscriptionsView)))
            .AddPolicy("Permission:Subscriptions.Manage", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.SubscriptionsManage)))
            .AddPolicy("Permission:Audit.View", policy => policy.Requirements.Add(new PermissionRequirement(Permissions.AuditView)));

        return services;
    }
}
