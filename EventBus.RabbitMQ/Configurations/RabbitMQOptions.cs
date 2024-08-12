namespace EventBus.RabbitMQ.Configurations;

public class RabbitMQOptions : BaseEventOptions, IHasQueueArguments
{
    /// <summary>
    /// To enable using an RabbitMQ. Default value is "true".
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// The name of the queue to use in RabbitMQ. Default value is "DefaultQueue".
    /// </summary>
    public string QueueName { get; set; }
    
    /// <summary>
    /// To enable using an inbox for storing all received events before handling. Default value is "false".
    /// </summary>
    public bool UseInbox { get; set; } = false;

    /// <summary>
    /// Optional queue arguments, also known as "x-arguments" because of their field name in the AMQP 0-9-1 protocol, is a map (dictionary) of arbitrary key/value pairs that can be provided by clients when a queue is declared.
    /// </summary>
    public Dictionary<string, object> QueueArguments { get; set; } = new();

    internal TEventOptions Clone<TEventOptions>() where TEventOptions : BaseEventOptions, new()
    {
        var options = new TEventOptions();
        options.OverwriteSettings(this);

        return options;
    }
    
    internal override void OverwriteSettings(BaseEventOptions settings)
    {
        if (settings is not null)
        {
            base.OverwriteSettings(settings);

            if (settings is IHasQueueArguments hasQueueArguments)
            {
                foreach (var argument in hasQueueArguments.QueueArguments)
                {
                    if (QueueArguments.ContainsKey(argument.Key))
                        QueueArguments[argument.Key] = argument.Value;
                    else
                        QueueArguments.Add(argument.Key, argument.Value);
                }
            }
        }
    }
}