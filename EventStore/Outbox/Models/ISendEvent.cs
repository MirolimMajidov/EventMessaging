namespace EventStore.Outbox.Models;

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