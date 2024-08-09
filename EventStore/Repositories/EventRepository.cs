using EventStore.Inbox.Configurations;

namespace EventStore.Repositories;

internal abstract class EventRepository : IEventRepository
{
    protected string TableName { get; }
    protected string ConnectionString { get; }

    public EventRepository(InboxOrOutboxStructure settings)
    {
        TableName = settings.TableName;
        ConnectionString = settings.ConnectionString;
    }
}