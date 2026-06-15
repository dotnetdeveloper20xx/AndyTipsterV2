using AndyTipster.Application.Users.DTOs;

namespace AndyTipster.Application.Users.Services;

/// <summary>
/// Service interface for admin user management operations.
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Gets a paginated, filtered, and searchable list of users.
    /// </summary>
    Task<UserListResponse> GetUsersAsync(UserListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed user information by ID.
    /// </summary>
    Task<UserDetailResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a read-only impersonation token for the specified user.
    /// </summary>
    Task<ImpersonateResponse?> ImpersonateUserAsync(Guid userId, Guid actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk action on the specified users with failure reporting.
    /// </summary>
    Task<BulkActionResponse> ExecuteBulkActionAsync(BulkActionRequest request, Guid actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspends a user account and revokes all active sessions/tokens within 5 seconds.
    /// </summary>
    Task<bool> SuspendUserAsync(Guid userId, Guid actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports filtered user data as CSV bytes.
    /// </summary>
    Task<byte[]> ExportUsersAsync(UserListRequest request, CancellationToken cancellationToken = default);
}
