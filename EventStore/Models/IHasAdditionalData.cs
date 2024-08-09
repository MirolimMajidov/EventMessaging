namespace EventStore.Models;

public interface IHasAdditionalData
{
    /// <summary>
    /// Gets or sets the additional data of the event. The data structure is similar to headers, but since it does not use in header while publishing, we need to split that.
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; internal set; }
}