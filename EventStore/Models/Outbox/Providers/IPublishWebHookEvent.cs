namespace EventStore.Models.Outbox.Providers;

/// <summary>
/// An interface to define an event publisher of type WebHook
/// </summary>
public interface IPublishWebHookEvent<TSendEvent> : IPublishEvent<TSendEvent>
    where TSendEvent : class, ISendEvent
{
}