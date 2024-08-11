namespace EventStorage.Models;

public interface IEvent
{
    /// <summary>
    /// Id of event
    /// </summary>
    public Guid EventId { get; }
}