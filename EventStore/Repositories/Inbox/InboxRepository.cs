using EventStore.Inbox.Configurations;
using EventStore.Models.Inbox;

namespace EventStore.Repositories.Inbox;

internal class InboxRepository : EventRepository<InboxEvent>, IInboxRepository
{
    public InboxRepository(InboxOrOutboxStructure settings) : base(settings)
    {
    }
}