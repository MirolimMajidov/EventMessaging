namespace EventBus.RabbitMQ.Configurations;

internal interface IHasQueueArguments
{
    /// <summary>
    /// Optional queue arguments, also known as "x-arguments" because of their field name in the AMQP 0-9-1 protocol, is a map (dictionary) of arbitrary key/value pairs that can be provided by clients when a queue is declared.
    /// </summary>
    public Dictionary<string, object> QueueArguments { get; set; }
}