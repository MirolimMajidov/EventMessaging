using EventStore.Configurations;
using EventStore.Outbox.Models;
using EventStore.Repositories;

namespace EventStore.Outbox.Repositories;

internal class OutboxRepository : EventRepository<OutboxEvent>, IOutboxRepository
{
    public OutboxRepository(InboxOrOutboxStructure settings) : base(settings)
    {
    }
}