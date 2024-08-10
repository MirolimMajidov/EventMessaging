namespace EventStore.Models.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type Email
/// </summary>
public interface IPublishEmailEvent<TSendEvent> : IPublishEvent<TSendEvent>
    where TSendEvent : class, ISendEvent
{
}