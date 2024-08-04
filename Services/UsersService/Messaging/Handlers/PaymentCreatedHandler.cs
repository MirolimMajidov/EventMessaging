using EventBus.RabbitMQ.Subscribers;
using UsersService.Messaging.Events;

namespace UsersService.Messaging.Handlers;

public class PaymentCreatedHandler : IEventSubscriberHandler<PaymentCreated>
{
    private readonly ILogger<PaymentCreatedHandler> _logger;

    public PaymentCreatedHandler(ILogger<PaymentCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(PaymentCreated @event, Dictionary<string, object>? eventHeaders)
    {
        _logger.LogInformation("EventId ({EventId}): Payment has been created for {UserId} id with the {PaymentId} payment id, for {Amount} amount.", @event.EventId,
            @event.UserId, @event.PaymentId, @event.Amount);

        return Task.CompletedTask;
    }
}