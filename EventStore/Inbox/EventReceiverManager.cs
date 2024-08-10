using System.Text.Json;
using EventStore.Inbox.Configurations;
using EventStore.Models;
using EventStore.Models.Outbox;
using EventStore.Models.Outbox.Providers;
using EventStore.Repositories.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStore.Inbox;

/// <summary>
/// Manager of events publisher
/// </summary>
internal class EventReceiverManager : IEventReceiverManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventReceiverManager> _logger;
    private readonly InboxOrOutboxStructure _settings;

    private readonly Dictionary<string, (Type eventType, Type eventHandlerType, string providerType, bool
        hasHeaders, bool hasAdditionalData)> _publishers;

    private const string PublisherMethodName = nameof(IPublishEvent<ISendEvent>.Publish);
    private static readonly int TryAfterMinutes = (int)TimeSpan.FromDays(1).TotalMinutes;
    
    private static readonly Type hasHeadersType = typeof(IHasHeaders);
    private static readonly Type hasAdditionalDataType = typeof(IHasAdditionalData);
    
    public EventReceiverManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<EventReceiverManager>>();
        _settings = serviceProvider.GetRequiredService<InboxAndOutboxSettings>().Outbox;
        _publishers = new();
    }

    /// <summary>
    /// Registers a subscriber 
    /// </summary>
    /// <param name="typeOfEventSender">Event type which we want to use to send</param>
    /// <param name="typeOfEventPublisher">Publisher type of the event which we want to publish event</param>
    /// <param name="providerType">Provider type of event publisher</param>
    public void AddPublisher(Type typeOfEventSender, Type typeOfEventPublisher, EventProviderType providerType)
    {
        var eventName = typeOfEventSender.Name;
        if (!_publishers.ContainsKey(eventName))
        {
            var hasHeaders = hasHeadersType.IsAssignableFrom(typeOfEventSender);
            var hasAdditionalData = hasAdditionalDataType.IsAssignableFrom(typeOfEventSender);

            _publishers.Add(eventName,
                (typeOfEventSender, typeOfEventPublisher, providerType.ToString(), hasHeaders, hasAdditionalData));
        }
    }

    public async Task ExecuteUnprocessedEvents(CancellationToken stoppingToken)
    {
        var semaphore = new SemaphoreSlim(_settings.MaxConcurrency);
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var eventsToPublish = await outboxRepository.GetUnprocessedEventsAsync();

        stoppingToken.ThrowIfCancellationRequested();
        var tasks = eventsToPublish.Select(async eventToPublish =>
        {
            await semaphore.WaitAsync(stoppingToken);
            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                await ExecuteEventPublisher(eventToPublish, scope);
            }
            catch
            {
                eventToPublish.Failed(_settings.TryCount, _settings.TryAfterMinutes);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);

        await outboxRepository.UpdateEventsAsync(eventsToPublish);
    }

    public async Task<bool> ExecuteEventPublisher(IOutboxEvent @event, IServiceScope serviceScope)
    {
        try
        {
            if (_publishers.TryGetValue(@event.EventName,
                    out (Type eventType, Type eventHandlerType, string providerType, bool hasHeaders, bool
                    hasAdditionalData) info))
            {
                if (@event.Provider == info.providerType)
                {
                    _logger.LogTrace("Executing the {EventType} outbox event with ID {EventId} to publish.",
                        @event.EventName, @event.Id);

                    var eventToPublish = JsonSerializer.Deserialize(@event.Payload, info.eventType) as ISendEvent;
                    if (info.hasHeaders && @event.Headers is not null)
                        ((IHasHeaders)eventToPublish).Headers =
                            JsonSerializer.Deserialize<Dictionary<string, string>>(@event.Headers);

                    if (info.hasAdditionalData && @event.AdditionalData is not null)
                        ((IHasAdditionalData)eventToPublish).AdditionalData =
                            JsonSerializer.Deserialize<Dictionary<string, string>>(@event!.AdditionalData);

                    var eventHandlerSubscriber = serviceScope.ServiceProvider.GetRequiredService(info.eventHandlerType);

                    var publisherMethod = info.eventHandlerType.GetMethod(PublisherMethodName);
                    var executedSuccessfully = await (Task<bool>)publisherMethod.Invoke(eventHandlerSubscriber,
                        [eventToPublish, @event.EventPath]);
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
                        "The {EventType} outbox event with ID {EventId} requested to publish with {ProviderType} provider, but that is configured to publish with the {ConfiguredProviderType} provider.",
                        @event.EventName, @event.Id, @event.Provider, info.providerType);
                }
            }
            else
            {
                @event.Failed(0, TryAfterMinutes);
                _logger.LogWarning(
                    "No publish provider configured for the {EventType} outbox event with ID: {EventId}.",
                    @event.EventName, @event.Id);
            }

            return false;
        }
        catch (Exception e)
        {
            @event.Failed(_settings.TryCount, _settings.TryAfterMinutes);
            _logger.LogError(e, "Error while publishing event with ID: {EventId}", @event.Id);
            throw;
        }
    }
}