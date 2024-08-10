using EventStore.Inbox.Configurations;
using EventStore.Models.Outbox;

namespace EventStore.Repositories.Outbox;

internal class OutboxRepository : EventRepository<OutboxEvent>, IOutboxRepository
{
    public OutboxRepository(InboxOrOutboxStructure settings) : base(settings)
    {
    }
}