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

    public Task Handle(UserUpdated @event)
    {
        if (@event.Headers?.TryGetValue("TraceId", out object traceId) == true)
        {
        }
        
        return Task.CompletedTask;
    }
}