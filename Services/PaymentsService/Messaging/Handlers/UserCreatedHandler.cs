using EventBus.RabbitMQ.Subscribers;
using PaymentsService.Messaging.Events.Subscibers;

namespace PaymentsService.Messaging.Handlers;

public class UserCreatedHandler : IEventSubscriberHandler<UserCreated>
{
    private readonly ILogger<UserCreatedHandler> _logger;

    public UserCreatedHandler(ILogger<UserCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreated @event)
    {
        if (@event.Headers?.TryGetValue("TraceId", out var traceId) == true)
        {
        }

        _logger.LogInformation("EventId ({EventId}): {UserName} user is created with the {UserId} id", @event.EventId,
            @event.UserName, @event.UserId);

        return Task.CompletedTask;
    }
}