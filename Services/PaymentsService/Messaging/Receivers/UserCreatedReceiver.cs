using EventBus.RabbitMQ.Subscribers.Models;
using PaymentsService.Messaging.Events.Subscribers;

namespace PaymentsService.Messaging.Receivers;

public class UserCreatedReceiver : IEventSubscriberReceiver<UserCreated>
{
    private readonly ILogger<UserCreatedReceiver> _logger;

    public UserCreatedReceiver(ILogger<UserCreatedReceiver> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(UserCreated @event)
    {
        if (@event.Headers?.TryGetValue("TraceId", out var traceId) == true)
        {
        }

        _logger.LogInformation("EventId ({EventId}): {UserName} user is created with the {UserId} id", @event.EventId,
            @event.UserName, @event.UserId);

        return await Task.FromResult(true);
    }
}