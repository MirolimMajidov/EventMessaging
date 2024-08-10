namespace EventStore.Models.Outbox;

internal class OutboxEvent : IOutboxEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Provider { get; set; }
    public string EventName { get; init; }
    public string EventPath { get; init; }
    public string Payload { get; internal set; }
    public string Headers { get; internal set; }
    public string AdditionalData { get; internal set; }
    public DateTime CreatedAt { get; } = DateTime.Now;
    public int TryCount { get; set; }
    public DateTime TryAfterAt { get; set; } = DateTime.Now;
    public DateTime? ProcessedAt { get; set; }
    public void Process()
    {
        ProcessedAt = DateTime.Now;
    }
}