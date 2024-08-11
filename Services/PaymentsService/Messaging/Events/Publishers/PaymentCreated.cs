using EventBus.RabbitMQ.Publishers.Models;
using EventStorage.Outbox.Models;

namespace PaymentsService.Messaging.Events.Publishers;

public record PaymentCreated : PublishEvent, IHasAdditionalData
{
    public Guid PaymentId { get; set; }

    public Guid UserId { get; set; }

    public double Amount { get; set; }
    
    public Dictionary<string, string> AdditionalData { get; set; }
}