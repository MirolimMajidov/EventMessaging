namespace EventBus.RabbitMQ.Configurations;

internal static class RabbitMQOptionsConstant
{
    /// <summary>
    /// The host name of the RabbitMQ server. Default value is "localhost".
    /// </summary>
    public const string HostName = "localhost";

    /// <summary>
    /// The port number on which RabbitMQ is running. Default value is "5672".
    /// </summary>
    public const int HostPort = 5672;

    /// <summary>
    /// The virtual host of the RabbitMQ server. Default value is "/".
    /// </summary>
    public const string VirtualHost = "/";

    /// <summary>
    /// The username to connect to RabbitMQ. Default value is "guest".
    /// </summary>
    public const string UserName = "guest";

    /// <summary>
    /// The password for the specified username to connect to RabbitMQ. Default value is "guest".
    /// </summary>
    public const string Password = "guest";

    /// <summary>
    /// The name of the exchange to use in RabbitMQ. Default value is "DefaultExchange".
    /// </summary>
    public const string ExchangeName = "DefaultExchange";

    /// <summary>
    /// The type of the exchange to use in RabbitMQ. Default value is "topic". It can be one of "direct", "fanout", or "topic".
    /// </summary>
    public const string ExchangeType = "topic";
    
    /// <summary>
    /// The name of the queue to use in RabbitMQ. Default value is "DefaultQueue".
    /// </summary>
    public const string QueueName = "DefaultQueue";
    
    /// <summary>
    /// The routing key to use for message routing in RabbitMQ. Default value is "DefaultRoutingKey".
    /// </summary>
    public const string RoutingKey = "DefaultRoutingKey";

    /// <summary>
    /// Retry count to publish event. Default value is "1".
    /// </summary>
    public const int RetryPublishCount = 1;

    /// <summary>
    /// Clone/Copying settings
    /// </summary>
    /// <returns>Returns a new copy of settings</returns>
    public static RabbitMQOptions CreateDefaultRabbitMQOptions()
    {
        return new RabbitMQOptions
        {
            HostName = HostName,
            HostPort = HostPort,
            VirtualHost = VirtualHost,
            UserName = UserName,
            Password = Password,
            ExchangeName = ExchangeName,
            ExchangeType = ExchangeType,
            QueueName = QueueName,
            RoutingKey = RoutingKey,
            RetryPublishCount = RetryPublishCount
        };
    }
}