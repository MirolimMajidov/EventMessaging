namespace EventBus.RabbitMQ.Configurations;

internal static class RabbitMQOptionsConstant
{
    /// <summary>
    /// The host name of the RabbitMQ server. Default value is "localhost".
    /// </summary>
    const string HostName = "localhost";

    /// <summary>
    /// The port number on which RabbitMQ is running. Default value is "5672".
    /// </summary>
    const int HostPort = 5672;

    /// <summary>
    /// The virtual host of the RabbitMQ server. Default value is "/".
    /// </summary>
    const string VirtualHost = "/";

    /// <summary>
    /// The username to connect to RabbitMQ. Default value is "guest".
    /// </summary>
    const string UserName = "guest";

    /// <summary>
    /// The password for the specified username to connect to RabbitMQ. Default value is "guest".
    /// </summary>
    const string Password = "guest";

    /// <summary>
    /// The name of the exchange to use in RabbitMQ. Default value is "DefaultExchange".
    /// </summary>
    const string ExchangeName = "DefaultExchange";

    /// <summary>
    /// The type of the exchange to use in RabbitMQ. Default value is "topic". It can be one of "direct", "fanout", or "topic".
    /// </summary>
    const string ExchangeType = "topic";

    /// <summary>
    /// The name of the queue to use in RabbitMQ. Default value is "DefaultQueue".
    /// </summary>
    const string QueueName = "DefaultQueue";

    /// <summary>
    /// The routing key to use for message routing in RabbitMQ. Default value is "DefaultRoutingKey".
    /// </summary>
    const string RoutingKey = "DefaultRoutingKey";

    /// <summary>
    /// Retry count to connect to the RabbitMQ. Default value is "3".
    /// </summary>
    const int RetryConnectionCount = 3;

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
            RetryConnectionCount = RetryConnectionCount
        };
    }
}