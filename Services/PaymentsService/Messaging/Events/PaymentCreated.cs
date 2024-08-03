using EventBus.RabbitMQ.Publishers;

namespace PaymentsService.Messaging.Events;

public class PaymentCreated : IEventPublisher
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime CreatedAt { get; } = DateTime.Now;

    public Guid PaymentId { get; set; }

    public Guid UserId { get; set; }

    public double Amount { get; set; }
}