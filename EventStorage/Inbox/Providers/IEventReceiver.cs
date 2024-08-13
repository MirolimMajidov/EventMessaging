using EventStorage.Inbox.Models;

namespace EventStorage.Inbox.Providers;

/// <summary>
/// Base interface to implement receiving events functionality for all providers
/// </summary>
public interface IEventReceiver<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
    /// <summary>
    /// To receive a message 
    /// </summary>
    /// <param name="event">Received an event</param>
    /// <returns>Return true if it executes successfully</returns>
    Task<bool> Receive(TReceiveEvent @event);
}