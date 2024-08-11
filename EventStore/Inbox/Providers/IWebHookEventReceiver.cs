using EventStore.Inbox.Models;
using EventStore.Inbox.Providers;

namespace EventStore.Models.Inbox.Providers;

/// <summary>
/// An interface to define an event receiver of type WebHook
/// </summary>
public interface IWebHookEventReceiver<TReceiveEvent> : IEventReceiver<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
}