using EventStorage.Inbox.Models;
using EventStorage.Inbox.Providers;

namespace EventStorage.Models.Inbox.Providers;

/// <summary>
/// An interface to define an event receiver of type WebHook
/// </summary>
public interface IWebHookEventReceiver<TReceiveEvent> : IEventReceiver<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
}