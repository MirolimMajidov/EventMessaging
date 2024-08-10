namespace EventStore.Models.Outbox;

/// <summary>
/// An interface for determine events to send
/// </summary>
public interface ISendEvent
{
    /// <summary>
    /// Id of event
    /// </summary>
    public Guid EventId { get; }
}