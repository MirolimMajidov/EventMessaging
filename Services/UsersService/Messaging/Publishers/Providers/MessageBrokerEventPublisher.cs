using EventBus.RabbitMQ.Publishers.Managers;
using EventBus.RabbitMQ.Publishers.Models;
using EventStorage.Outbox.Models;
using EventStorage.Outbox.Providers;

namespace UsersService.Messaging.Publishers.Providers;

public class MessageBrokerEventPublisher : IMessageBrokerEventPublisher
{
    private readonly IEventPublisherManager _eventPublisher;
    
    public MessageBrokerEventPublisher(IEventPublisherManager eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    
    public async Task<bool> Publish(ISendEvent @event, string eventPath)
    {
        _eventPublisher.Publish((IPublishEvent)@event);
        return await Task.FromResult(true);
    }
}