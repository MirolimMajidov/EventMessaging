using EventStore.Inbox;
using EventStore.Inbox.Configurations;
using EventStore.Repositories.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventStore.BackgroundServices;

//TODO: I need to add another service to remove an old processed events
internal class EventsReceiverService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IEventReceiverManager _eventReceiverManager;
    private readonly ILogger<EventsReceiverService> _logger;
    private readonly TimeSpan _timeToDelay;

    public EventsReceiverService(IServiceProvider services, IEventReceiverManager eventReceiverManager,
        InboxAndOutboxSettings settings, ILogger<EventsReceiverService> logger)
    {
        _services = services;
        _eventReceiverManager = eventReceiverManager;
        _logger = logger;
        _timeToDelay = TimeSpan.FromSeconds(settings.Outbox.SecondsToDelay);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
        inboxRepository.CreateTableIfNotExists();

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _eventReceiverManager.ExecuteUnprocessedEvents(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Something is wrong while receiving/updating an inbox events. Happened at: {time}",
                    DateTimeOffset.Now);
            }
            finally
            {
                await Task.Delay(_timeToDelay, stoppingToken);
            }
        }
    }
}