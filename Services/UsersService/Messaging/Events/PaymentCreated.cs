using EventBus.RabbitMQ.Subscribers;

namespace UsersService.Messaging.Events;

public class PaymentCreated : EventSubscriber
{
    public Guid PaymentId { get; init; }

    public Guid UserId { get; init; }

    public double Amount { get; init; }
}