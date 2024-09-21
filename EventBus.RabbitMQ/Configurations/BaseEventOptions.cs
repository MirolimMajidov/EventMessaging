using System.Text.Json;
using System.Text.Json.Serialization;
using EventBus.RabbitMQ.Models;

namespace EventBus.RabbitMQ.Configurations;

public abstract class BaseEventOptions
{
    /// <summary>
    /// The name of the event. By default, it will get an event name.
    /// </summary>
    public string EventTypeName { get; set; }

    /// <summary>
    /// The name of the queue to use in RabbitMQ.
    /// </summary>
    public string QueueName { get; set; }

    /// <summary>
    /// The routing key to use for message routing in RabbitMQ.
    /// </summary>
    public string RoutingKey { get; set; }
    
    /// <summary>
    /// The key of virtual host to find the host settings and use that to connect to the RabbitMQ.
    /// </summary>
    public string VirtualHostKey { get; set; }

    /// <summary>
    /// Naming police for serializing and deserializing properties of Event. Default value is "PascalCase". It can be one of "PascalCase", "CamelCase", "SnakeCaseLower", "SnakeCaseUpper", "KebabCaseLower", or "KebabCaseUpper".
    /// </summary>
    public PropertyNamingPolicyType? PropertyNamingPolicy { get; set; }
    
    /// <summary>
    /// Virtual host settings to connect to the RabbitMQ. It should set while loading application based on a <see cref="VirtualHostKey"/> value.
    /// </summary>
    internal RabbitMqHostSettings VirtualHostSettings { get; private set; }
    
    /// <summary>
    /// Set the virtual host and other unassigned settings.
    /// </summary>
    /// <param name="settings">Virtual host setting to use as a source</param>
    /// <param name="eventTypeName">Event type name set its value</param>
    internal void SetVirtualHostAndUnassignedSettings(RabbitMqHostSettings settings, string eventTypeName)
    {
        VirtualHostSettings = settings;
        
        if (string.IsNullOrEmpty(EventTypeName))
            EventTypeName = eventTypeName;
            
        if (string.IsNullOrEmpty(QueueName))
            QueueName = settings.QueueName;
            
        if (string.IsNullOrEmpty(RoutingKey))
            RoutingKey = settings.RoutingKey;
            
        if (PropertyNamingPolicy is null)
            PropertyNamingPolicy = settings.PropertyNamingPolicy;
    }

    private JsonSerializerOptions _jsonSerializerOptions;
    /// <summary>
    /// Gets JsonSerializerOptions to use on naming police for serializing and deserializing properties of Event 
    /// </summary>
    /// <returns></returns>
    public JsonSerializerOptions GetJsonSerializer()
    {
        if (_jsonSerializerOptions is not null)
            return _jsonSerializerOptions;

        _jsonSerializerOptions = new JsonSerializerOptions
            { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        switch (PropertyNamingPolicy)
        {
            case PropertyNamingPolicyType.CamelCase:
                _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                break;
            case PropertyNamingPolicyType.SnakeCaseLower:
                _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                break;
            case PropertyNamingPolicyType.SnakeCaseUpper:
                _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;
                break;
            case PropertyNamingPolicyType.KebabCaseLower:
                _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower;
                break;
            case PropertyNamingPolicyType.KebabCaseUpper:
                _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper;
                break;
        }

        return _jsonSerializerOptions;
    }
}