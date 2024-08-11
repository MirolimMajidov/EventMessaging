using EventBus.RabbitMQ.Subscribers.Models;
using PaymentsService.Messaging.Events.Subscribers;

namespace PaymentsService.Messaging.Receivers;

public class UserDeleted : IEventSubscriber<Events.Subscribers.UserDeleted>
{
    private readonly ILogger<UserDeleted> _logger;

    public UserDeleted(ILogger<UserDeleted> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(Events.Subscribers.UserDeleted @event)
    {
        _logger.LogInformation("EventId ({EventId}): {UserName} user is deleted, the User id is {UserId}", @event.EventId,
            @event.UserName, @event.UserId);

        return await Task.FromResult(true);
    }
}