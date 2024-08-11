using EventStore.Inbox.Models;

namespace EventStore.Inbox.Providers;

/// <summary>
/// An interface to define an event receiver of type Email
/// </summary>
public interface IEmailEventReceiver<TReceiveEvent> : IEventReceiver<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
}