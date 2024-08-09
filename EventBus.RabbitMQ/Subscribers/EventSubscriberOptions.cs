using EventBus.RabbitMQ.Configurations;

namespace EventBus.RabbitMQ.Subscribers;

public class EventSubscriberOptions : BaseEventOptions, IHasQueueArguments
{
    /// <summary>
    /// The name of the queue to use in RabbitMQ. Default value is "DefaultQueue".
    /// </summary>
    public string QueueName { get; set; }
    
    /// <summary>
    /// The name of the event. By default, it will get an event name.
    /// </summary>
    public string EventTypeName { get; set; }

    /// <summary>
    /// Optional queue arguments, also known as "x-arguments" because of their field name in the AMQP 0-9-1 protocol, is a map (dictionary) of arbitrary key/value pairs that can be provided by clients when a queue is declared.
    /// </summary>
    public Dictionary<string, object> QueueArguments { get; set; } = new(); 
    
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