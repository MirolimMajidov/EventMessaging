namespace EventStore.Models;

internal record EventMetadata
{
    /// <summary>
    /// Gets or sets the header data of the event.
    /// </summary>
    public Dictionary<string, object> Headers { get; internal set; }

    /// <summary>
    /// Gets or sets the additional data of the event. The data structure is similar to headers, but since it does not use in header while publishing, we need to split that.
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; internal set; }
}