using EventBus.RabbitMQ.Subscribers.Models;
using OrdersService.Messaging.Events;

namespace OrdersService.Messaging.Subscribers;

public class UserUpdatedSubscriber : IEventSubscriber<UserUpdated>
{
    private readonly ILogger<UserUpdatedSubscriber> _logger;

    public UserUpdatedSubscriber(ILogger<UserUpdatedSubscriber> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(UserUpdated @event)
    {
        if (@event.Headers?.TryGetValue("TraceId", out string traceId) == true)
        {
        }
        
        return await Task.FromResult(true);
    }
}