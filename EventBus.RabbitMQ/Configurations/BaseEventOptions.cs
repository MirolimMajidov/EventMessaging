using System.Reflection;

namespace EventBus.RabbitMQ.Configurations;

public abstract class BaseEventOptions
{
    /// <summary>
    /// The host name of the RabbitMQ server. Default value is "localhost".
    /// </summary>
    public string? HostName { get; set; }

    /// <summary>
    /// The port number on which RabbitMQ is running. Default value is "5672".
    /// </summary>
    public int? HostPort { get; set; }

    /// <summary>
    /// The virtual host of the RabbitMQ server. Default value is "/".
    /// </summary>
    public string? VirtualHost { get; set; }

    /// <summary>
    /// The username to connect to RabbitMQ. Default value is "guest".
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// The password for the specified username to connect to RabbitMQ. Default value is "guest".
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// The name of the exchange to use in RabbitMQ. Default value is "DefaultExchange".
    /// </summary>
    public string? ExchangeName { get; set; }

    /// <summary>
    /// The type of the exchange to use in RabbitMQ. Default value is "topic". It can be one of "direct", "fanout", or "topic".
    /// </summary>
    public string? ExchangeType { get; set; }

    /// <summary>
    /// The routing key to use for message routing in RabbitMQ. Default value is "DefaultRoutingKey".
    /// </summary>
    public string? RoutingKey { get; set; }
    
    /// <summary>
    /// Overwriting settings
    /// </summary>
    /// <param name="settings">Settings to use for overwriting the main settings if the settings parameter value is not null</param>
    /// <returns></returns>
    internal void OverwriteSettings(BaseEventOptions? settings)
    {
        if (settings is not null)
        {
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = settings[property.Name];
                if (value is not null)
                    property.SetValue(this, value);
            }
        }
    }

    internal object? this[string propertyName]
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
}