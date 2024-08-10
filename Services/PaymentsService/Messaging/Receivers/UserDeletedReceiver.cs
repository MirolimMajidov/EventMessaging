using EventBus.RabbitMQ.Subscribers.Models;
using PaymentsService.Messaging.Events.Subscribers;

namespace PaymentsService.Messaging.Receivers;

public class UserDeletedReceiver : IEventSubscriberReceiver<UserDeleted>
{
    private readonly ILogger<UserDeletedReceiver> _logger;

    public UserDeletedReceiver(ILogger<UserDeletedReceiver> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(UserDeleted @event)
    {
        _logger.LogInformation("EventId ({EventId}): {UserName} user is deleted, the User id is {UserId}", @event.EventId,
            @event.UserName, @event.UserId);

        return await Task.FromResult(true);
    }
}