using EventBus.RabbitMQ.Subscribers;
using PaymentsService.Messaging.Events;

namespace Payments.Service.Messaging.Handlers;

public class UserDeletedHandler : IEventSubscriberHandler<UserDeleted>
{
    private readonly ILogger<UserDeletedHandler> _logger;

    public UserDeletedHandler(ILogger<UserDeletedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserDeleted @event)
    {
        _logger.LogInformation("EventId id ({EventId}): {UserName} user is deleted, the id is {UserId}", @event.UserId,
            @event.UserName, @event.UserId);

        return Task.CompletedTask;
    }
}