namespace AndyTipster.Domain.Common;

/// <summary>
/// Base class for all domain entities providing common properties.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
