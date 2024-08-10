using EventBus.RabbitMQ.Subscribers.Models;
using OrdersService.Messaging.Events;

namespace OrdersService.Messaging.Receivers;

public class UserUpdatedReceiver : IEventSubscriberReceiver<UserUpdated>
{
    private readonly ILogger<UserUpdatedReceiver> _logger;

    public UserUpdatedReceiver(ILogger<UserUpdatedReceiver> logger)
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