using EventBus.RabbitMQ.Subscribers.Models;

namespace PaymentsService.Messaging.Events.Subscribers;

public record UserCreated : SubscribeEvent
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}