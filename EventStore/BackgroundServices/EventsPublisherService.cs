using EventStore.Inbox.Configurations;
using EventStore.Outbox;
using EventStore.Repositories.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventStore.BackgroundServices;

internal class EventsPublisherService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IEventPublisherManager _eventPublisherManager;
    private readonly ILogger<EventsPublisherService> _logger;
    private readonly InboxOrOutboxStructure _outboxSettings;

    public EventsPublisherService(IServiceProvider services, IEventPublisherManager eventPublisherManager,
        InboxAndOutboxSettings settings, ILogger<EventsPublisherService> logger)
    {
        _services = services;
        _eventPublisherManager = eventPublisherManager;
        _logger = logger;
        _outboxSettings = settings.Outbox;
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
        var semaphore = new SemaphoreSlim(_outboxSettings.MaxConcurrency);
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            try
            {
                var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var eventsToPublish = await outboxRepository.GetUnprocessedEventsAsync();

                var tasks = eventsToPublish.Select(async eventToPublish =>
                {
                    await semaphore.WaitAsync(stoppingToken);
                    try
                    {
                        await _eventPublisherManager.ExecuteEventPublisher(eventToPublish, scope);
                    }
                    catch
                    {
                        eventToPublish.Failed(_outboxSettings.TryCount, _outboxSettings.TryAfterMinutes);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToList();

                await Task.WhenAll(tasks);

                await outboxRepository.UpdateEventsAsync(eventsToPublish);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Something is wrong while publishing/updating an outbox events. Happened at: {time}",
                    DateTimeOffset.Now);
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(_outboxSettings.SecondsToDelay), stoppingToken);
            }
        }
    }
}