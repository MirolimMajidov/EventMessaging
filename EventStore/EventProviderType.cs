namespace EventStore;

/// <summary>
/// Provider type of event
/// </summary>
public enum EventProviderType
{
    RabbitMQ,
    SMS,
    WebHook,
    Email,
    Unknown
}