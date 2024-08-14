using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventBus.RabbitMQ.Configurations;

public abstract class BaseEventOptions
{
    /// <summary>
    /// The host name of the RabbitMQ server. Default value is "localhost".
    /// </summary>
    public string HostName { get; set; }

    /// <summary>
    /// The port number on which RabbitMQ is running. Default value is "5672".
    /// </summary>
    public int? HostPort { get; set; }

    /// <summary>
    /// The virtual host of the RabbitMQ server. Default value is "/".
    /// </summary>
    public string VirtualHost { get; set; }

    /// <summary>
    /// The username to connect to RabbitMQ. Default value is "guest".
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// The password for the specified username to connect to RabbitMQ. Default value is "guest".
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// The name of the exchange to use in RabbitMQ. Default value is "DefaultExchange".
    /// </summary>
    public string ExchangeName { get; set; }

    /// <summary>
    /// The type of the exchange to use in RabbitMQ. Default value is "topic". It can be one of "direct", "fanout", or "topic".
    /// </summary>
    public string ExchangeType { get; set; }

    /// <summary>
    /// The routing key to use for message routing in RabbitMQ. Default value is "DefaultRoutingKey".
    /// </summary>
    public string RoutingKey { get; set; }

    /// <summary>
    /// Retry count to connect to the RabbitMQ. Default value is "5".
    /// </summary>
    public int? RetryConnectionCount { get; set; }

    /// <summary>
    /// Naming police for serializing and deserializing properties of Event. Default value is "PascalCase". It can be one of "PascalCase", "CamelCase", "SnakeCaseLower", "SnakeCaseUpper", "KebabCaseLower", or "KebabCaseUpper".
    /// </summary>
    public string PropertyNamingPolicy { get; set; }
    
    /// <summary>
    /// Indicates whether TLS/SSL should be used for the connection.
    /// When set to true, the connection will be secured using TLS/SSL.
    /// </summary>
    public bool? UseTls { get; set; }

    /// <summary>
    /// The file path to the client's certificate used for TLS/SSL authentication.
    /// This certificate is typically used to prove the identity of the client to the server.
    /// </summary>
    public string ClientCertPath { get; set; }

    /// <summary>
    /// The file path to the client's private key associated with the client certificate.
    /// This key is used to establish a secure TLS/SSL connection and must correspond to the certificate specified in <see cref="ClientCertPath"/>.
    /// </summary>
    public string ClientKeyPath { get; set; }

    /// <summary>
    /// Overwriting settings
    /// </summary>
    /// <param name="settings">Settings to use for overwriting the main settings if the settings parameter value is not null</param>
    /// <returns></returns>
    internal virtual void OverwriteSettings(BaseEventOptions settings)
    {
        if (settings is not null)
        {
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !(p.PropertyType is { IsGenericType: true, IsValueType: false }));
            foreach (var property in properties)
            {
                var value = settings[property.Name];
                if (value is not null)
                    property.SetValue(this, value);
            }
        }
    }

    internal object this[string propertyName]
    {
        get
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            return propertyInfo?.GetValue(this);
        }
        set
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            if (propertyInfo != null)
                propertyInfo.SetValue(this, Convert.ChangeType(value, propertyInfo.PropertyType), null);
        }
    }

    /// <summary>
    /// Gets JsonSerializerOptions to use on naming police for serializing and deserializing properties of Event 
    /// </summary>
    /// <returns></returns>
    public JsonSerializerOptions GetJsonSerializer()
    {
        var settings = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        switch (PropertyNamingPolicy)
        {
            case nameof(JsonNamingPolicy.CamelCase):
                settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                break;
            case nameof(JsonNamingPolicy.SnakeCaseLower):
                settings.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                break;
            case nameof(JsonNamingPolicy.SnakeCaseUpper):
                settings.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper;
                break;
            case nameof(JsonNamingPolicy.KebabCaseLower):
                settings.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower;
                break;
            case nameof(JsonNamingPolicy.KebabCaseUpper):
                settings.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper;
                break;
        }

        return settings;
    }
}