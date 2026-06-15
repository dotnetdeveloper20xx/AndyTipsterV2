namespace AndyTipster.Infrastructure.Authorization;

/// <summary>
/// Constants for all system permissions. Used for policy-based authorization.
/// </summary>
public static class Permissions
{
    // User permissions
    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersEdit = "Users.Edit";
    public const string UsersDelete = "Users.Delete";
    public const string UsersImpersonate = "Users.Impersonate";

    // Role permissions
    public const string RolesView = "Roles.View";
    public const string RolesCreate = "Roles.Create";
    public const string RolesEdit = "Roles.Edit";
    public const string RolesDelete = "Roles.Delete";
    public const string RolesAssign = "Roles.Assign";

    // Plan permissions
    public const string PlansView = "Plans.View";
    public const string PlansCreate = "Plans.Create";
    public const string PlansEdit = "Plans.Edit";
    public const string PlansDelete = "Plans.Delete";

    // Tip permissions
    public const string TipsView = "Tips.View";
    public const string TipsCreate = "Tips.Create";
    public const string TipsEdit = "Tips.Edit";
    public const string TipsDelete = "Tips.Delete";

    // CMS permissions
    public const string CmsView = "CMS.View";
    public const string CmsCreate = "CMS.Create";
    public const string CmsEdit = "CMS.Edit";
    public const string CmsDelete = "CMS.Delete";
    public const string CmsPublish = "CMS.Publish";

    // Analytics permissions
    public const string AnalyticsView = "Analytics.View";

    // Subscription permissions
    public const string SubscriptionsView = "Subscriptions.View";
    public const string SubscriptionsManage = "Subscriptions.Manage";

    // Audit permissions
    public const string AuditView = "Audit.View";

    /// <summary>
    /// Returns all permission constants for seeding and registration.
    /// </summary>
    public static IReadOnlyList<string> All => new[]
    {
        UsersView, UsersCreate, UsersEdit, UsersDelete, UsersImpersonate,
        RolesView, RolesCreate, RolesEdit, RolesDelete, RolesAssign,
        PlansView, PlansCreate, PlansEdit, PlansDelete,
        TipsView, TipsCreate, TipsEdit, TipsDelete,
        CmsView, CmsCreate, CmsEdit, CmsDelete, CmsPublish,
        AnalyticsView,
        SubscriptionsView, SubscriptionsManage,
        AuditView
    };
}
