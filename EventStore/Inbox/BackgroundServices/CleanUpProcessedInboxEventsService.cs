using EventStore.BackgroundServices;
using EventStore.Configurations;
using EventStore.Inbox.Models;
using EventStore.Inbox.Repositories;
using EventStore.Outbox.BackgroundServices;
using Microsoft.Extensions.Logging;

namespace EventStore.Inbox.BackgroundServices;

internal class CleanUpProcessedInboxEventsService : CleanUpProcessedEventsService<IInboxRepository, InboxEvent>
{
    public CleanUpProcessedInboxEventsService(IServiceProvider services,
        InboxAndOutboxSettings settings, ILogger<EventsPublisherService> logger) : base(services, settings.Inbox,
        logger)
    {
    }
}