namespace EventStore.Models.Inbox.Providers;

/// <summary>
/// An interface for determine a receiver of events. It may use for Unknown type.
/// </summary>
public interface IReceiveEvent<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
    /// <summary>
    /// To receive a message 
    /// </summary>
    /// <param name="event">Received an event</param>
    /// <returns>Return true if it executes successfully</returns>
    Task<bool> Receive(TReceiveEvent @event);
}