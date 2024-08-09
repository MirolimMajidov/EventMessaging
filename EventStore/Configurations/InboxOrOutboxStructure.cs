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
    /// The database connection string of Inbox/Outbox for storing or reading all received/sending events.
    /// </summary>
    public string ConnectionString { get; set; }
}