using EventBus.RabbitMQ.Subscribers;
using OrdersService.Messaging.Events;

namespace OrdersService.Messaging.Handlers;

public class UserUpdatedHandler : IEventSubscriberHandler<UserUpdated>
{
    private readonly ILogger<UserUpdatedHandler> _logger;

    public UserUpdatedHandler(ILogger<UserUpdatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserUpdated @event, Dictionary<string, object>? eventHeaders)
    {
        _logger.LogInformation("EventId ({EventId}): User which has {UserId} id, renamed from {OldName} to {NewName}", @event.EventId,
            @event.UserId, @event.OldUserName, @event.NewUserName);

        return Task.CompletedTask;
    }
}