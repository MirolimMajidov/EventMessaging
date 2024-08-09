namespace EventStore;

/// <summary>
/// Represents an event type for storing and reading.
/// </summary>
public interface IBaseEventBox
{
    /// <summary>
    /// Gets or sets the ID of the record.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets provider of the event. It can be RabbitMQ, SMS, Webhook, Email, Unknown
    /// </summary>
    public EventProviderType Provider { get; }

    /// <summary>
    /// Gets or sets the path of the event. It can be event name or pouting key, or URL.
    /// </summary>
    string EventPath { get; init; }

    /// <summary>
    /// Gets or sets the payload of the event in JSON format.
    /// </summary>
    string Payload { get; init; }

    /// <summary>
    /// Gets or sets the metadata of the event in JSON format. It may have headers and additional data.
    /// </summary>
    string Metadata { get; init; }

    /// <summary>
    /// Gets or sets the type of stored event. It can be Outbox or Inbox.
    /// </summary>
    string StoredEventType { get; init; }

    /// <summary>
    /// Gets the creation time of the event.
    /// </summary>
    DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Gets the count of attempts to process the event.
    /// </summary>
    int TryCount { get; set; }

    /// <summary>
    /// Gets the count of attempts to process the event.
    /// </summary>
    public DateTimeOffset TryAfterAt { get; set; }

    /// <summary>
    /// Gets or sets the status of the event. When 0, the event is not sent/handled; when 1, the event is sent/handled.
    /// </summary>
    bool Processed { get; set; }
}