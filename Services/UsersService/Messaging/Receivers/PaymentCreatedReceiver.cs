using EventBus.RabbitMQ.Subscribers.Models;
using UsersService.Messaging.Events.Subscribers;
using UsersService.Services;

namespace UsersService.Messaging.Receivers;

public class PaymentCreatedReceiver : IEventSubscriberReceiver<PaymentCreated>
{
    private readonly ILogger<PaymentCreatedReceiver> _logger;
    private readonly IUserService _service;

    public PaymentCreatedReceiver(ILogger<PaymentCreatedReceiver> logger, IUserService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task<bool> Receive(PaymentCreated @event)
    {
        _logger.LogInformation("EventId ({EventId}): Payment has been created for {UserId} user id with the {PaymentId} payment id, for {Amount} amount.", @event.EventId,
            @event.UserId, @event.PaymentId, @event.Amount);

        return await Task.FromResult(true);
    }
}