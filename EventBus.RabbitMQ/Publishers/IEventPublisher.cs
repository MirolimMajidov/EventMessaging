namespace EventBus.RabbitMQ.Publishers;

/// <summary>
/// Base interface for all publisher classes
/// </summary>
public interface IEventPublisher : IBaseEvent
{
    /// <summary>
    /// Adding a value to the Headers of publishing event/message.
    /// </summary>
    /// <param name="name">Unique name to add to the event headers</param>
    /// <param name="value">Value of adding header</param>
    /// <returns>Returns true if added successfully</returns>
    public bool TryAddHeader(string name, object value);

    /// <summary>
    /// To get added header values
    /// </summary>
    /// <returns>Returns all added items</returns>
    public IDictionary<string, object> GetHeaders();
}