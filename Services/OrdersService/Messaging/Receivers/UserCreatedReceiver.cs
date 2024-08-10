using EventBus.RabbitMQ.Subscribers.Models;
using OrdersService.Messaging.Events;

namespace OrdersService.Messaging.Receivers;

public class UserCreatedReceiver : IEventSubscriberReceiver<UserCreated>
{
    private readonly ILogger<UserCreatedReceiver> _logger;

    public UserCreatedReceiver(ILogger<UserCreatedReceiver> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(UserCreated @event)
    {
        _logger.LogInformation("EventId ({EventId}): {UserName} user is created with the {UserId} id", @event.EventId,
            @event.UserName, @event.UserId);

        return await Task.FromResult(true);
    }
}