using EventBus.RabbitMQ.Subscribers;
using UsersService.Messaging.Events;
using UsersService.Messaging.Events.Subscribers;
using UsersService.Repositories;
using UsersService.Services;

namespace UsersService.Messaging.Handlers;

public class PaymentCreatedHandler : IEventSubscriberHandler<PaymentCreated>
{
    private readonly ILogger<PaymentCreatedHandler> _logger;
    private readonly IUserService _service;

    public PaymentCreatedHandler(ILogger<PaymentCreatedHandler> logger, IUserService service)
    {
        _logger = logger;
        _service = service;
    }

    public Task Handle(PaymentCreated @event)
    {
        _logger.LogInformation("EventId ({EventId}): Payment has been created for {UserId} user id with the {PaymentId} payment id, for {Amount} amount.", @event.EventId,
            @event.UserId, @event.PaymentId, @event.Amount);

        return Task.CompletedTask;
    }
}