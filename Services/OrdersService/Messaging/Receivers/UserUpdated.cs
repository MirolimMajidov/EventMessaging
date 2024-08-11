using EventBus.RabbitMQ.Subscribers.Models;
using OrdersService.Messaging.Events;

namespace OrdersService.Messaging.Receivers;

public class UserUpdated : IEventSubscriber<Events.UserUpdated>
{
    private readonly ILogger<UserUpdated> _logger;

    public UserUpdated(ILogger<UserUpdated> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(Events.UserUpdated @event)
    {
        if (@event.Headers?.TryGetValue("TraceId", out string traceId) == true)
        {
        }
        
        return await Task.FromResult(true);
    }
}