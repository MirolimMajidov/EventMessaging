namespace EventStore.Inbox.Configurations;

public class InboxAndOutbox
{
    /// <summary>
    /// To enable using an inbox for storing all received events. Default value is "true".
    /// </summary>
    public bool IsEnabledInbox { get; set; } = true;

    /// <summary>
    /// The table name of Inbox for storing all received events. Default value is "Inbox".
    /// </summary>
    public string InboxTableName { get; set; } = "Inbox";
    
    /// <summary>
    /// To enable using an outbox for storing all received events. Default value is "true".
    /// </summary>
    public bool IsEnabledOutbox { get; set; } = true;
    
    /// <summary>
    /// The table name of Outbox for storing all publishing events. Default value is "Outbox".
    /// </summary>
    public string OutboxTableName { get; set; } = "Outbox";
}