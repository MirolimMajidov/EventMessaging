namespace EventStore.Outbox;

public interface IEventSender
{
    /// <summary>
    /// To store sending an event to the database
    /// </summary>
    /// <param name="event">Event to send</param>
    /// <typeparam name="eventProviderType">Provider type of sending event</typeparam>
    /// <typeparam name="TSendEvent">Event type that must implement from the ISendEvent</typeparam>
    public void Send<TSendEvent>(TSendEvent @event, EventProviderType eventProviderType) where TSendEvent : ISendEvent;
}