using EventBus.RabbitMQ.Publishers;
using EventStore.Models.Outbox.Providers;
using UsersService.Messaging.Events.Publishers;

namespace UsersService.Messaging.Publishers;

public class CreatedUserPublisher : IPublishRabbitMQEvent<UserCreated>
{
    private readonly IEventPublisherManager _eventPublisher;

    public CreatedUserPublisher(IEventPublisherManager eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    
    public async Task<bool> Publish(UserCreated @event, string eventPath)
    {
        _eventPublisher.Publish(@event);
        
        return await Task.FromResult(true);
    }
}