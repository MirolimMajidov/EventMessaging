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

    public Task Handle(UserCreated @event, Dictionary<string, object> eventHeaders)
    {
        if (eventHeaders?.TryGetValue("TraceId", out object traceId) == true)
        {
        }

        _logger.LogInformation("EventId ({EventId}): {UserName} user is created with the {UserId} id", @event.EventId,
            @event.UserName, @event.UserId);

        return Task.CompletedTask;
    }
}