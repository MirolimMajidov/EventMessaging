using EventStorage.Outbox.Models;

namespace EventStorage.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type SMS
/// </summary>
public interface ISmsEventPublisher<TSendEvent> : IEventPublisher<TSendEvent>
    where TSendEvent : class, ISendEvent
{
    
}