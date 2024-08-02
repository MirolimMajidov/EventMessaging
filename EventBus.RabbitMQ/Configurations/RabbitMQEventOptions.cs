namespace EventBus.RabbitMQ.Configurations;

public class RabbitMQEventOptions
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
    /// The name of the queue to use in RabbitMQ. Default value is "DefaultQueue".
    /// </summary>
    public string? QueueName { get; set; }

    /// <summary>
    /// The routing key to use for message routing in RabbitMQ. Default value is "DefaultRoutingKey".
    /// </summary>
    public string? RoutingKey { get; set; }

    /// <summary>
    /// Clone/Copying settings
    /// </summary>
    /// <returns>Returns a new copy of settings</returns>
    internal RabbitMQEventOptions Clone()
    {
        return new RabbitMQEventOptions
        {
            HostName = HostName,
            HostPort = HostPort,
            VirtualHost = VirtualHost,
            UserName = UserName,
            Password = Password,
            ExchangeName = ExchangeName,
            ExchangeType = ExchangeType,
            QueueName = QueueName,
            RoutingKey = RoutingKey
        };
    }

    /// <summary>
    /// Overwriting settings
    /// </summary>
    /// <param name="settings">Settings to use for overwriting the main settings if the settings parameter value is not null</param>
    /// <returns></returns>
    internal void OverwriteSettings(RabbitMQEventOptions? settings)
    {
        if (settings is not null)
        {
            var properties = typeof(RabbitMQEventOptions).GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(settings);
                if (value is not null)
                    property.SetValue(this, value);
            }
        }
    }
}