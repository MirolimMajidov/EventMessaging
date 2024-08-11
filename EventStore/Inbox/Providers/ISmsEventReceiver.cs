using EventStore.Inbox.Models;

namespace EventStore.Inbox.Providers;

/// <summary>
/// An interface to define an event receiver of type SMS
/// </summary>
public interface ISmsEventReceiver<TReceiveEvent> : IEventReceiver<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
    
}