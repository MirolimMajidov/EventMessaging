namespace EventBus.RabbitMQ.Subscribers;

internal class EventSubscriberManager
{
    /// <summary>
    /// Dictionary collection to store all event and event handler information
    /// </summary>
    private readonly Dictionary<string, (Type EventType, Type EventHandlerType)> _subscribers = new();
}