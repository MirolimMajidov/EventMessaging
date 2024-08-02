namespace EventBus.RabbitMQ.Configurations;

public class RabbitMQOptions : BaseEventOptions
{
    /// <summary>
    /// The name of the queue to use in RabbitMQ. Default value is "DefaultQueue".
    /// </summary>
    public string? QueueName { get; set; }
    
    /// <summary>
    /// Retry count to publish event
    /// </summary>
    public int? RetryPublishCount { get; init; }

    internal TEventOptions Clone<TEventOptions>() where TEventOptions: BaseEventOptions, new()
    {
        var options  = new TEventOptions();
        options.OverwriteSettings(this);

        return options;
    }
}