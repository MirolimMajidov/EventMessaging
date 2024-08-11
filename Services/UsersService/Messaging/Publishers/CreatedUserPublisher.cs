using EventBus.RabbitMQ.Publishers.Managers;
using EventStorage.Outbox.Providers;
using UsersService.Messaging.Events.Publishers;

namespace UsersService.Messaging.Publishers;

public class CreatedUserPublisher : IRabbitMqEventPublisher<UserCreated>
{
    // private readonly IEventPublisherManager _eventPublisher;
    //
    // public CreatedUserPublisher(IEventPublisherManager eventPublisher)
    // {
    //     _eventPublisher = eventPublisher;
    // }
    
    public async Task<bool> Publish(UserCreated @event, string eventPath)
    {
        // _eventPublisher.Publish(@event);
        //Add you logic
        return await Task.FromResult(true);
    }
}