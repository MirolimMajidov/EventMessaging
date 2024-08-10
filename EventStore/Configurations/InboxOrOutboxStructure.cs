namespace EventStore.Inbox.Configurations;

public record InboxOrOutboxStructure
{
    /// <summary>
    /// To enable using an inbox/outbox for storing all received/sending events. Default value is "true".
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// The table name of Inbox/Outbox for storing all received/sending events. Default value is "Inbox" if it is for Inbox, otherwise "Outbox".
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Maximum concurrency tasks to execute received/publishing events. Default value is "10".
    /// </summary>
    public int MaxConcurrency { get; set; } = 10;

    /// <summary>
    /// For increasing the TryAfterAt when the TryCount is higher than the value. Default value is "10".
    /// </summary>
    public int TryCount { get; set; } = 10;

    /// <summary>
    /// For increasing the TryAfterAt to amount of minutes if the event fails. Default value is "5".
    /// </summary>
    public int TryAfterMinutes { get; set; } = 5;

    /// <summary>
    /// Seconds to delay before publishing/receiving events. Default value is "1".
    /// </summary>
    public int SecondsToDelay { get; set; } = 1;

    /// <summary>
    /// The database connection string of Inbox/Outbox for storing or reading all received/sending events.
    /// </summary>
    public string ConnectionString { get; set; }
}