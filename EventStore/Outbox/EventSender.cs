namespace EventStore.Outbox;

public class EventSender : IEventSender
{
    public void Send<TSendEvent>(TSendEvent @event, EventProviderType eventProviderType) where TSendEvent : ISendEvent
    {
        
    }
}