namespace EventBus.RabbitMQ.Subscribers;

internal interface IEventConsumerService
{
    /// <summary>
    /// Registers a subscriber 
    /// </summary>
    /// <param name="eventInfo">Event and handler types with the settings which we want to subscribe</param>
    public void AddSubscriber((Type, Type, EventSubscriberOptions) eventInfo);
}