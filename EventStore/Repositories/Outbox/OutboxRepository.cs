using EventStore.Inbox.Configurations;

namespace EventStore.Repositories.Outbox;

internal class OutboxRepository : EventRepository, IOutboxRepository
{
    public OutboxRepository(InboxOrOutboxStructure settings) : base(settings)
    {
    }
}