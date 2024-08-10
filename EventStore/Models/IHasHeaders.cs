namespace EventStore.Models;

public interface IHasHeaders
{
    /// <summary>
    /// Gets or sets the header data of the event.
    /// </summary>
    public Dictionary<string, object> Headers { get; set; }
}