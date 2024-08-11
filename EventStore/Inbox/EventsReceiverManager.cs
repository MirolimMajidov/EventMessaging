using System.Text.Json;
using EventStore.Configurations;
using EventStore.Inbox.Models;
using EventStore.Inbox.Providers;
using EventStore.Inbox.Repositories;
using EventStore.Models;
using EventStore.Outbox.Models;
using EventStore.Outbox.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStore.Inbox;

/// <summary>
/// Manager of events receiver
/// </summary>
internal class EventsReceiverManager : IEventsReceiverManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventsReceiverManager> _logger;
    private readonly InboxOrOutboxStructure _settings;

    private readonly Dictionary<string, (Type eventType, Type eventReceiverType, string providerType, bool hasHeaders, bool hasAdditionalData)> _receivers;

    private const string ReceiverMethodName = nameof(IEventReceiver<IReceiveEvent>.Receive);
    private static readonly int TryAfterMinutes = (int)TimeSpan.FromDays(1).TotalMinutes;
    
    private static readonly Type HasHeadersType = typeof(IHasHeaders);
    private static readonly Type HasAdditionalDataType = typeof(IHasAdditionalData);
    
    public EventsReceiverManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<EventsReceiverManager>>();
        _settings = serviceProvider.GetRequiredService<InboxAndOutboxSettings>().Outbox;
        _receivers = new();
    }

    /// <summary>
    /// Registers a receiver 
    /// </summary>
    /// <param name="typeOfReceiveEvent">Event type which we want to use to receive</param>
    /// <param name="typeOfEventReceiver">Receiver type of the event which we want to receiver event</param>
    /// <param name="providerType">Provider type of received event</param>
    public void AddReceiver(Type typeOfReceiveEvent, Type typeOfEventReceiver, EventProviderType providerType)
    {
        var eventName = typeOfReceiveEvent.Name;
        if (!_receivers.ContainsKey(eventName))
        {
            var hasHeaders = HasHeadersType.IsAssignableFrom(typeOfReceiveEvent);
            var hasAdditionalData = HasAdditionalDataType.IsAssignableFrom(typeOfReceiveEvent);

            _receivers.Add(eventName,
                (typeOfReceiveEvent, typeOfEventReceiver, providerType.ToString(), hasHeaders, hasAdditionalData));
        }
    }

    public async Task ExecuteUnprocessedEvents(CancellationToken stoppingToken)
    {
        var semaphore = new SemaphoreSlim(_settings.MaxConcurrency);
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
        var eventsToReceive = await repository.GetUnprocessedEventsAsync();

        stoppingToken.ThrowIfCancellationRequested();
        var tasks = eventsToReceive.Select(async eventToReceive =>
        {
            await semaphore.WaitAsync(stoppingToken);
            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                await ExecuteEventReceiver(eventToReceive, scope);
            }
            catch
            {
                eventToReceive.Failed(_settings.TryCount, _settings.TryAfterMinutes);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);

        await repository.UpdateEventsAsync(eventsToReceive);
    }

    private async Task<bool> ExecuteEventReceiver(IInboxEvent @event, IServiceScope serviceScope)
    {
        try
        {
            if (_receivers.TryGetValue(@event.EventName,
                    out (Type eventType, Type eventReceiverType, string providerType, bool hasHeaders, bool hasAdditionalData) info))
            {
                if (@event.Provider == info.providerType)
                {
                    _logger.LogTrace("Executing the {EventType} inbox event with ID {EventId} to receive.",
                        @event.EventName, @event.Id);

                    var eventToReceive = JsonSerializer.Deserialize(@event.Payload, info.eventType) as IReceiveEvent;
                    if (info.hasHeaders && @event.Headers is not null)
                        ((IHasHeaders)eventToReceive).Headers =
                            JsonSerializer.Deserialize<Dictionary<string, string>>(@event.Headers);

                    if (info.hasAdditionalData && @event.AdditionalData is not null)
                        ((IHasAdditionalData)eventToReceive).AdditionalData =
                            JsonSerializer.Deserialize<Dictionary<string, string>>(@event!.AdditionalData);

                    var eventReceiver = serviceScope.ServiceProvider.GetRequiredService(info.eventReceiverType);

                    var receiveMethod = info.eventReceiverType.GetMethod(ReceiverMethodName);
                    var executedSuccessfully = await (Task<bool>)receiveMethod.Invoke(eventReceiver,
                        [eventToReceive]);
                    if (executedSuccessfully)
                        @event.Processed();
                    else
                        @event.IncreaseTryCount();

                    return executedSuccessfully;
                }
                else
                {
                    @event.Failed(0, TryAfterMinutes);
                    _logger.LogError(
                        "The {EventType} inbox event with ID {EventId} requested to receive with {ProviderType} provider, but that is configured to receive with the {ConfiguredProviderType} provider.",
                        @event.EventName, @event.Id, @event.Provider, info.providerType);
                }
            }
            else
            {
                @event.Failed(0, TryAfterMinutes);
                _logger.LogWarning(
                    "No publish provider configured for the {EventType} inbox event with ID: {EventId}.",
                    @event.EventName, @event.Id);
            }

            return false;
        }
        catch (Exception e)
        {
            @event.Failed(_settings.TryCount, _settings.TryAfterMinutes);
            _logger.LogError(e, "Error while receiving event with ID: {EventId}", @event.Id);
            throw;
        }
    }
}