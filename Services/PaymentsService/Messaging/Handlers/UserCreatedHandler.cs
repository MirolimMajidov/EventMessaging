using EventBus.RabbitMQ.Subscribers;
using PaymentsService.Messaging.Events;

namespace Payments.Service.Messaging.Handlers;

public class UserCreatedHandler : IEventSubscriberHandler<UserCreated>
{
    private readonly ILogger<UserCreatedHandler> _logger;

    public UserCreatedHandler(ILogger<UserCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreated @event)
    {
        _logger.LogInformation("EventId id ({EventId}): {UserName} user is created with the {UserId} id", @event.UserId,
            @event.UserName, @event.UserId);

        return Task.CompletedTask;
    }
}