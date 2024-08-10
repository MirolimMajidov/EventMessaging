namespace EventStore.Models.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type SMS
/// </summary>
public interface IPublishSMSEvent<TSendEvent> : IPublishEvent<TSendEvent>
    where TSendEvent : class, ISendEvent
{
    
}