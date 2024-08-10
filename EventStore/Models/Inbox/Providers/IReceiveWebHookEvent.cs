namespace EventStore.Models.Inbox.Providers;

/// <summary>
/// An interface to define an event receiver of type WebHook
/// </summary>
public interface IReceiveWebHookEvent<TReceiveEvent> : IReceiveEvent<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
}