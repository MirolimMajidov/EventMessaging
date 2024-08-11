using EventStore.Configurations;
using EventStore.Outbox.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventStore.Outbox.BackgroundServices;

//TODO: I need to add another service to remove an old processed events
internal class EventsPublisherService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IEventPublisherManager _eventPublisherManager;
    private readonly ILogger<EventsPublisherService> _logger;
    private readonly TimeSpan _timeToDelay;

    public EventsPublisherService(IServiceProvider services, IEventPublisherManager eventPublisherManager,
        InboxAndOutboxSettings settings, ILogger<EventsPublisherService> logger)
    {
        _services = services;
        _eventPublisherManager = eventPublisherManager;
        _logger = logger;
        _timeToDelay = TimeSpan.FromSeconds(settings.Outbox.SecondsToDelay);
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
                await _eventPublisherManager.ExecuteUnprocessedEvents(stoppingToken);
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