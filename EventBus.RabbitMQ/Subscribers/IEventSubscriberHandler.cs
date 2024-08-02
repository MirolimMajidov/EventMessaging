namespace EventBus.RabbitMQ.Subscribers;

/// <summary>
/// Base interface for all event subscriber handler classes
/// </summary>
public interface IEventSubscriberHandler<TEventSubscriber>
    where TEventSubscriber :  class, IEventSubscriber
{
    /// <summary>
    /// To receive a message 
    /// </summary>
    /// <param name="event">Sent event by event bus</param>
    Task Handle(TEventSubscriber @event);
}