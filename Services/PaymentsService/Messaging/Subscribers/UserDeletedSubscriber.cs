using EventBus.RabbitMQ.Subscribers.Models;
using PaymentsService.Messaging.Events.Subscribers;

namespace PaymentsService.Messaging.Subscribers;

public class UserDeletedSubscriber : IEventSubscriber<UserDeleted>
{
    private readonly ILogger<UserDeletedSubscriber> _logger;

    public UserDeletedSubscriber(ILogger<UserDeletedSubscriber> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(UserDeleted @event)
    {
        _logger.LogInformation("Id ({Id}): {UserName} user is deleted, the User id is {UserId}", @event.EventId,
            @event.UserName, @event.UserId);

        return await Task.FromResult(true);
    }
}