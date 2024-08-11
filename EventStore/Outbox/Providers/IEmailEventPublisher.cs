using EventStore.Outbox.Models;

namespace EventStore.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type Email
/// </summary>
public interface IEmailEventPublisher<TSendEvent> : IEventPublisher<TSendEvent>
    where TSendEvent : class, ISendEvent
{
}