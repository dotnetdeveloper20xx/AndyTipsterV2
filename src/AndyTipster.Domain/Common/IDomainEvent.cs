namespace AndyTipster.Domain.Common;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
