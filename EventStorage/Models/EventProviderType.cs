namespace EventStorage.Models;

/// <summary>
/// Provider type of event
/// </summary>
public enum EventProviderType
{
    RabbitMq,
    Sms,
    WebHook,
    Email,
    Unknown
}