namespace EventStore.Models.Inbox;

/// <summary>
/// An interface for determine events to receive
/// </summary>
public interface IReceiveEvent
{
    /// <summary>
    /// Id of event
    /// </summary>
    public Guid EventId { get; }
}