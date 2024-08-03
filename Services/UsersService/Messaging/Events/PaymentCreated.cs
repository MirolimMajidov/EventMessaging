using EventBus.RabbitMQ.Subscribers;

namespace UsersService.Messaging.Events;

public class PaymentCreated : IEventSubscriber
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.Now;

    public Guid PaymentId { get; set; }

    public Guid UserId { get; set; }

    public double Amount { get; set; }
}