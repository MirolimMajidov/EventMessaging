using EventBus.RabbitMQ.Publishers;

namespace PaymentsService.Messaging.Events.Publishers;

public class PaymentCreated : EventPublisher
{
    public Guid PaymentId { get; set; }

    public Guid UserId { get; set; }

    public double Amount { get; set; }
}