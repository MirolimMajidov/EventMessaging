using EventStore.Outbox;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventStore.BackgroundServices;

public class EventsPublisherService : BackgroundService
{
    private readonly ILogger<EventsPublisherService> _logger;

    public EventsPublisherService(IEventPublisherProvider[] publisherProviders, ILogger<EventsPublisherService> logger)
    {
        _logger = logger;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}