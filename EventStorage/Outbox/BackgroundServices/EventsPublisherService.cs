using EventStorage.Configurations;
using EventStorage.Outbox.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventStorage.Outbox.BackgroundServices;

internal class EventsPublisherService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IEventsPublisherManager _eventsPublisherManager;
    private readonly ILogger<EventsPublisherService> _logger;
    private readonly TimeSpan _timeToDelay;

    public EventsPublisherService(IServiceProvider services, IEventsPublisherManager eventsPublisherManager,
        InboxAndOutboxSettings settings, ILogger<EventsPublisherService> logger)
    {
        _services = services;
        _eventsPublisherManager = eventsPublisherManager;
        _logger = logger;
        _timeToDelay = TimeSpan.FromSeconds(settings.Outbox.SecondsToDelayProcessEvents);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        outboxRepository.CreateTableIfNotExists();

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _eventsPublisherManager.ExecuteUnprocessedEvents(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Something is wrong while publishing/updating an outbox events. Happened at: {time}",
                    DateTimeOffset.Now);
            }
            finally
            {
                await Task.Delay(_timeToDelay, stoppingToken);
            }
        }
    }
}