using EventStore.Configurations;
using EventStore.Inbox.Models;
using EventStore.Repositories;

namespace EventStore.Inbox.Repositories;

internal class InboxRepository : EventRepository<InboxEvent>, IInboxRepository
{
    public InboxRepository(InboxOrOutboxStructure settings) : base(settings)
    {
    }
}