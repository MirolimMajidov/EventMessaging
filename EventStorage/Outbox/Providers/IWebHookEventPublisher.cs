using EventStorage.Outbox.Models;

namespace EventStorage.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type WebHook
/// </summary>
public interface IWebHookEventPublisher<TSendEvent> : IEventPublisher<TSendEvent>
    where TSendEvent : class, ISendEvent
{
}