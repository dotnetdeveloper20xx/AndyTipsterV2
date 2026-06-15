namespace AndyTipster.Application.Common;

/// <summary>
/// Abstraction for the application database context.
/// Implemented in the Infrastructure layer.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
