using EventStore.Outbox.Models;

namespace EventStore.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type SMS
/// </summary>
public interface IPublishSMSEvent<TSendEvent> : IPublishEvent<TSendEvent>
    where TSendEvent : class, ISendEvent
{
    
}