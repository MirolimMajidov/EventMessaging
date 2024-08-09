namespace EventStore;

internal record EventMetadata
{
    /// <summary>
    /// Gets or sets the header data of the event in JSON format.
    /// </summary>
    public string Headers { get; init; }

    /// <summary>
    /// Gets or sets the additional data of the event in JSON format. The data structure is similar to headers, but since it does not use in header while publishing, we  need to split that.
    /// </summary>
    public string AdditionalData { get; init; }
}