using EventBus.RabbitMQ.Subscribers;
using PaymentsService.Messaging.Events.Subscibers;

namespace PaymentsService.Messaging.Handlers;

public class UserDeletedHandler : IEventSubscriberHandler<UserDeleted>
{
    private readonly ILogger<UserDeletedHandler> _logger;

    public UserDeletedHandler(ILogger<UserDeletedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserDeleted @event)
    {
        _logger.LogInformation("EventId ({EventId}): {UserName} user is deleted, the User id is {UserId}", @event.EventId,
            @event.UserName, @event.UserId);

        return Task.CompletedTask;
    }
}