namespace EventStore.Models.Inbox.Providers;

/// <summary>
/// An interface to define an event receiver of type Email
/// </summary>
public interface IReceiveEmailEvent<TReceiveEvent> : IReceiveEvent<TReceiveEvent>
    where TReceiveEvent : class, IReceiveEvent
{
}