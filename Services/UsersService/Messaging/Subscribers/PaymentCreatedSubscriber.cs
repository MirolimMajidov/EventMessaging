using EventBus.RabbitMQ.Subscribers.Models;
using UsersService.Messaging.Events.Subscribers;
using UsersService.Services;

namespace UsersService.Messaging.Subscribers;

public class PaymentCreatedSubscriber : IEventSubscriber<PaymentCreated>
{
    private readonly ILogger<PaymentCreatedSubscriber> _logger;
    private readonly IUserService _service;

    public PaymentCreatedSubscriber(ILogger<PaymentCreatedSubscriber> logger, IUserService service)
    {
        _logger = logger;
        _service = service;
    }

    public async Task<bool> Receive(PaymentCreated @event)
    {
        _logger.LogInformation("Id ({Id}): Payment has been created for {UserId} user id with the {PaymentId} payment id, for {Amount} amount.", @event.EventId,
            @event.UserId, @event.PaymentId, @event.Amount);

        return await Task.FromResult(true);
    }
}