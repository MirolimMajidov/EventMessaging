namespace EventStore.Inbox.Configurations;

public class InboxAndOutboxOptions
{
    private readonly InboxAndOutbox _inboxAndOutbox;

    public InboxAndOutboxOptions(InboxAndOutbox inboxAndOutbox)
    {
        _inboxAndOutbox = inboxAndOutbox;
    }

    /// <summary>
    /// For enabling using an Inbox
    /// </summary>
    public void EnableInbox()
    {
        _inboxAndOutbox.IsEnabledInbox = true;
    }

    /// <summary>
    /// For disabling using an Inbox
    /// </summary>
    public void DisableInbox()
    {
        _inboxAndOutbox.IsEnabledInbox = false;
    }

    /// <summary>
    /// For changing default table name of Inbox
    /// </summary>
    public void ChangeInboxTableName(string tableName)
    {
        _inboxAndOutbox.InboxTableName = tableName;
    }

    /// <summary>
    /// For enabling using an Outbox
    /// </summary>
    public void EnableOutbox()
    {
        _inboxAndOutbox.IsEnabledOutbox = true;
    }

    /// <summary>
    /// For disabling using an Inbox
    /// </summary>
    public void DisableOutbox()
    {
        _inboxAndOutbox.IsEnabledOutbox = false;
    }

    /// <summary>
    /// For changing default table name of Outbox
    /// </summary>
    public void ChangeOutboxTableName(string tableName)
    {
        _inboxAndOutbox.OutboxTableName = tableName;
    }
}