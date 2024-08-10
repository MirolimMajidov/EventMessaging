using EventBus.RabbitMQ.Publishers;
using EventStore.Models.Outbox.Providers;
using UsersService.Messaging.Events.Publishers;

namespace UsersService.Messaging.Publishers;

public class CreatedUserPublisher : IPublishEvent<UserCreated>
{
    private readonly IEventPublisherManager _eventPublisher;

    public CreatedUserPublisher(IEventPublisherManager eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    
    public async Task<bool> Publish(UserCreated @event, string eventPath)
    {
        //Add your logic
        return await Task.FromResult(true);
    }
}