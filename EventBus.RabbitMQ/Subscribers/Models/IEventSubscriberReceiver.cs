using EventStore.Models.Inbox.Providers;

namespace EventBus.RabbitMQ.Subscribers.Models;

/// <summary>
/// Base interface for all event subscriber handler classes
/// </summary>
public interface IEventSubscriberReceiver<TEventSubscriber> : IReceiveRabbitMQEvent<TEventSubscriber>
    where TEventSubscriber : class, IEventSubscriber
{
}