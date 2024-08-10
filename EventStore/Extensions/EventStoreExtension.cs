using System.Reflection;
using EventStore.BackgroundServices;
using EventStore.Inbox.Configurations;
using EventStore.Models;
using EventStore.Models.Outbox.Providers;
using EventStore.Outbox;
using EventStore.Repositories.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStore.Extensions;

public static class EventStoreExtension
{
    /// <summary>
    /// Register the RabbitMQ settings as EventBus
    /// </summary>
    /// <param name="services">Services of DI</param>
    /// <param name="configuration">Configuration to get config</param>
    /// <param name="options">Options to overwrite default settings of Inbox and Outbox. </param>
    /// <param name="assemblies">Assemblies to find and load publisher and subscribers</param>
    public static void AddEventStore(this IServiceCollection services, IConfiguration configuration,
        Assembly[] assemblies,
        Action<InboxAndOutboxOptions> options = null)
    {
        var settings = GetDefaultSettings();
        if (!settings.Inbox.IsEnabled && !settings.Outbox.IsEnabled)
            return;

        if (settings.Inbox.TableName == settings.Outbox.TableName)
            throw new ArgumentNullException("The table name for the index and outbox events cannot be the same.");

        services.AddSingleton(settings);

        if (settings.Outbox.IsEnabled)
        {
            services.AddScoped<IEventSender, EventSender>();
            services.AddScoped<IOutboxRepository>(serviceProvider =>
            {
                var _defaultSettings = serviceProvider.GetRequiredService<InboxAndOutboxSettings>();
                var _reporitory = new OutboxRepository(_defaultSettings.Outbox);
                
                return _reporitory;
            });
            
            RegisterAllEventsOfOutboxToDI(services, assemblies);
            services.AddSingleton<IEventPublisherManager>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<EventPublisherManager>>();
                var _publisherManager = new EventPublisherManager(logger);
                RegisterAllEventsOfOutbox(_publisherManager, assemblies);
                    
                return _publisherManager;
            });
            
            services.AddHostedService<EventsPublisherService>();
        }

        InboxAndOutboxSettings GetDefaultSettings()
        {
            var defaultSettings = configuration.GetSection("InboxAndOutbox").Get<InboxAndOutboxSettings>() ?? new();
            if (defaultSettings.Inbox is null)
            {
                defaultSettings.Inbox = new();
                defaultSettings.Inbox.TableName = nameof(defaultSettings.Inbox);
            }

            if (defaultSettings.Outbox is null)
            {
                defaultSettings.Outbox = new();
                defaultSettings.Outbox.TableName = nameof(defaultSettings.Outbox);
            }

            var inboxAndOutboxOptions = new InboxAndOutboxOptions(defaultSettings);
            options?.Invoke(inboxAndOutboxOptions);

            if (defaultSettings.Inbox.IsEnabled && string.IsNullOrEmpty(defaultSettings.Inbox.TableName))
                throw new ArgumentNullException("If the inbox is enabled, the table name cannot be empty");

            if (defaultSettings.Outbox.IsEnabled && string.IsNullOrEmpty(defaultSettings.Outbox.TableName))
                throw new ArgumentNullException("If the outbox is enabled, the table name cannot be empty");

            return defaultSettings;
        }
    }

    #region Publisher

    private static void RegisterAllEventsOfOutbox(EventPublisherManager subscriberManager, Assembly[] assemblies)
    {
        var outboxEventTypes = GetSubscriberHandlerTypes(assemblies);

        foreach (var (eventType, publisherType, provider) in outboxEventTypes)
            subscriberManager.AddPublisher(eventType, publisherType, provider);
    }

    private static void RegisterAllEventsOfOutboxToDI(IServiceCollection services, Assembly[] assemblies)
    {
        var outboxEventTypes = GetSubscriberHandlerTypes(assemblies);
        foreach (var (_, publisherType, _) in outboxEventTypes)
            services.AddTransient(publisherType);
    }

    static Type eventPublisherType = typeof(IPublishEvent<>);
    static Type rabbitMQEventType = typeof(IPublishRabbitMQEvent<>);
    static Type emailEventType = typeof(IPublishEmailEvent<>);
    static Type smsEventType = typeof(IPublishSMSEvent<>);
    static Type webHookEventType = typeof(IPublishWebHookEvent<>);
    private static List<(Type eventType, Type publisherType, EventProviderType provider)> GetSubscriberHandlerTypes(Assembly[] assemblies)
    {
        List<(Type eventType, Type publisherType, EventProviderType provider)> subscriberHandlerTypes = new();
        if (assemblies is not null)
        {
            var allTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false });
            foreach (var publisherType in allTypes)
            {
                foreach (var implementedInterface in publisherType.GetInterfaces())
                {
                    if (implementedInterface.IsGenericType)
                    {
                        EventProviderType? provider = null;
                        var genericType = implementedInterface.GetGenericTypeDefinition();
                        if (genericType == eventPublisherType)
                            provider = EventProviderType.Unknown;
                        else if (genericType == rabbitMQEventType)
                            provider = EventProviderType.RabbitMQ;
                        else if (genericType == emailEventType)
                            provider = EventProviderType.Email;
                        else if (genericType == smsEventType)
                            provider = EventProviderType.Email;
                        else if (genericType == webHookEventType)
                            provider = EventProviderType.WebHook;

                        if(provider is null)
                            break;
                        
                        var eventType = implementedInterface.GetGenericArguments().Single();
                        subscriberHandlerTypes.Add((eventType, publisherType, (EventProviderType)provider));
                        
                        break;
                    }
                }
            }
        }

        return subscriberHandlerTypes;
    }

    #endregion
}