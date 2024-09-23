using EventBus.RabbitMQ.Subscribers.Models;
using OrdersService.Messaging.Events;

namespace OrdersService.Messaging.Subscribers;

public class UserCreatedSubscriber : IEventSubscriber<UserCreated>
{
    private readonly ILogger<UserCreatedSubscriber> _logger;

    public UserCreatedSubscriber(ILogger<UserCreatedSubscriber> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(UserCreated @event)
    {
        _logger.LogInformation("Id ({Id}): '{UserName}' user is created with the {UserId} id", @event.EventId,
            @event.UserName, @event.UserId);

        return await Task.FromResult(true);
    }
}