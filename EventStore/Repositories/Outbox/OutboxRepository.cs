using EventStore.Inbox.Configurations;
using Microsoft.Extensions.Logging;

namespace EventStore.Repositories.Outbox;

internal class OutboxRepository : EventRepository, IOutboxRepository
{
    public OutboxRepository(InboxOrOutboxStructure settings, ILogger<OutboxRepository> logger) : base(settings, logger)
    {
    }
}