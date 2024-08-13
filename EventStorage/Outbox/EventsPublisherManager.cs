using System.Text.Json;
using EventStorage.Configurations;
using EventStorage.Models;
using EventStorage.Outbox.Models;
using EventStorage.Outbox.Providers;
using EventStorage.Outbox.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStorage.Outbox;

/// <summary>
/// Manager of events publisher
/// </summary>
internal class EventsPublisherManager : IEventsPublisherManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventsPublisherManager> _logger;
    private readonly InboxOrOutboxStructure _settings;

    private readonly Dictionary<string, (Type typeOfEvent, Type typeOfPublisher, string provider, bool hasHeaders, bool
        hasAdditionalData, bool isGlobalPublisher)> _publishers;

    private const string PublisherMethodName = nameof(IEventPublisher.Publish);
    private static readonly int TryAfterOneDay = (int)TimeSpan.FromDays(1).TotalMinutes;

    public EventsPublisherManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<EventsPublisherManager>>();
        _settings = serviceProvider.GetRequiredService<InboxAndOutboxSettings>().Outbox;
        _publishers = new();
    }

    /// <summary>
    /// Registers a publisher 
    /// </summary>
    /// <param name="typeOfEventSender">Event type which we want to use to send</param>
    /// <param name="typeOfEventPublisher">Publisher type of the event which we want to publish event</param>
    /// <param name="providerType">Provider type of event publisher</param>
    /// <param name="hasHeaders">The event may have headers</param>
    /// <param name="hasAdditionalData">The event may have AdditionalData</param>
    /// <param name="isGlobalPublisher">Publisher of event is global publisher</param>
    public void AddPublisher(Type typeOfEventSender, Type typeOfEventPublisher, EventProviderType providerType,
        bool hasHeaders, bool hasAdditionalData, bool isGlobalPublisher)
    {
        var providerName = providerType.ToString();
        var publisherKey = GetPublisherKey(typeOfEventSender.Name, providerName);
        _publishers[publisherKey] = (typeOfEventSender, typeOfEventPublisher, providerName, hasHeaders,
            hasAdditionalData, isGlobalPublisher);
    }

    private string GetPublisherKey(string eventName, string providerName)
    {
        return $"{eventName}-{providerName}";
    }

    public async Task ExecuteUnprocessedEvents(CancellationToken stoppingToken)
    {
        var semaphore = new SemaphoreSlim(_settings.MaxConcurrency);
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var eventsToPublish = await repository.GetUnprocessedEventsAsync();

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

        await repository.UpdateEventsAsync(eventsToPublish);
    }

    private async Task<bool> ExecuteEventPublisher(IOutboxEvent @event, IServiceScope serviceScope)
    {
        try
        {
            var publisherKey = GetPublisherKey(@event.EventName, @event.Provider);
            if (_publishers.TryGetValue(publisherKey,
                    out (Type typeOfEvent, Type typeOfPublisher, string provider, bool hasHeaders, bool
                    hasAdditionalData, bool isGlobalPublisher) info))
            {
                _logger.LogTrace("Executing the {EventType} outbox event with ID {EventId} to publish.",
                    @event.EventName, @event.Id);

                var eventToPublish = JsonSerializer.Deserialize(@event.Payload, info.typeOfEvent) as ISendEvent;
                if (info.hasHeaders && @event.Headers is not null)
                    ((IHasHeaders)eventToPublish).Headers =
                        JsonSerializer.Deserialize<Dictionary<string, string>>(@event.Headers);

                if (info.hasAdditionalData && @event.AdditionalData is not null)
                    ((IHasAdditionalData)eventToPublish).AdditionalData =
                        JsonSerializer.Deserialize<Dictionary<string, string>>(@event!.AdditionalData);

                var eventHandlerSubscriber = serviceScope.ServiceProvider.GetRequiredService(info.typeOfPublisher);

                var publisherMethod = info.typeOfPublisher.GetMethod(PublisherMethodName);
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
                @event.Failed(0, TryAfterOneDay);
                _logger.LogWarning(
                    "The {EventType} outbox event with ID {EventId} requested to publish with {ProviderType} provider, but no publisher configured for this event.",
                    @event.EventName, @event.Id, @event.Provider);
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