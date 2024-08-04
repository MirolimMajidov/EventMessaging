using EventBus.RabbitMQ.Publishers;

namespace PaymentsService.Messaging.Events;

public class PaymentCreated : EventPublisher
{
    public Guid PaymentId { get; set; }

    public Guid UserId { get; set; }

    public double Amount { get; set; }
}