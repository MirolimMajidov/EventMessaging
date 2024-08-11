using EventStore.Outbox.Models;

namespace EventStore.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type WebHook
/// </summary>
public interface IWebHookEventSender<TSendEvent> : IEventSender<TSendEvent>
    where TSendEvent : class, ISendEvent
{
}