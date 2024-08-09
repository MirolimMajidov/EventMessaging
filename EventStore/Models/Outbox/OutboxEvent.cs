namespace EventStore.Models.Outbox;

internal class OutboxEvent : IOutboxEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public EventProviderType Provider { get; init; } = EventProviderType.RabbitMQ;
    public string EventPath { get; init; }
    public string Payload { get; init; }
    public string Metadata { get; init; }
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;
    public int TryCount { get; set; }
    public DateTimeOffset TryAfterAt { get; set; }
    public bool Processed { get; set; }
}