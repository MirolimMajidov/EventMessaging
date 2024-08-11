using EventBus.RabbitMQ.Subscribers.Models;
using OrdersService.Messaging.Events;

namespace OrdersService.Messaging.Receivers;

public class UserCreated : IEventSubscriber<Events.UserCreated>
{
    private readonly ILogger<UserCreated> _logger;

    public UserCreated(ILogger<UserCreated> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(Events.UserCreated @event)
    {
        _logger.LogInformation("EventId ({EventId}): {UserName} user is created with the {UserId} id", @event.EventId,
            @event.UserName, @event.UserId);

        return await Task.FromResult(true);
    }
}