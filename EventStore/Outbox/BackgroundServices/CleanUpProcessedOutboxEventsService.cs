using EventStore.BackgroundServices;
using EventStore.Configurations;
using EventStore.Outbox.Models;
using EventStore.Outbox.Repositories;
using Microsoft.Extensions.Logging;

namespace EventStore.Outbox.BackgroundServices;

internal class CleanUpProcessedOutboxEventsService : CleanUpProcessedEventsService<IOutboxRepository, OutboxEvent>
{
    public CleanUpProcessedOutboxEventsService(IServiceProvider services,
        InboxAndOutboxSettings settings, ILogger<EventsPublisherService> logger) : base(services, settings.Outbox,
        logger)
    {
    }
}