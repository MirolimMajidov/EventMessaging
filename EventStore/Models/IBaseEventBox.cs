namespace EventStore.Models;

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
    string Provider { get; }

    /// <summary>
    /// Gets or sets the name of the event.
    /// </summary>
    string EventName { get; }

    /// <summary>
    /// Gets or sets the path of the event. It can be pouting key, URL, or different value depend on provider type.
    /// </summary>
    string EventPath { get; }

    /// <summary>
    /// Gets or sets the payload of the event in JSON format.
    /// </summary>
    string Payload { get; }

    /// <summary>
    /// Gets or sets the headers of the event in JSON format.
    /// </summary>
    string Headers { get; }

    /// <summary>
    /// Gets or sets the additional data of the event in JSON format.
    /// </summary>
    string AdditionalData { get; }

    /// <summary>
    /// Gets the creation time of the event.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the count of attempts to process the event.
    /// </summary>
    int TryCount { get; set; }

    /// <summary>
    /// Gets the count of attempts to process the event.
    /// </summary>
    public DateTime TryAfterAt { get; set; }

    /// <summary>
    /// Gets the processed time of the event.
    /// </summary>
    DateTime? ProcessedAt { get; internal set; }

    /// <summary>
    /// For processing the event
    /// </summary>
    void Process();
}