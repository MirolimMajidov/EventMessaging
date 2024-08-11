using System.Reflection;
using EventStore.Configurations;
using EventStore.Inbox;
using EventStore.Inbox.BackgroundServices;
using EventStore.Inbox.Managers;
using EventStore.Inbox.Providers;
using EventStore.Inbox.Repositories;
using EventStore.Models;
using EventStore.Models.Inbox.Providers;
using EventStore.Outbox;
using EventStore.Outbox.BackgroundServices;
using EventStore.Outbox.Managers;
using EventStore.Outbox.Providers;
using EventStore.Outbox.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventStore.Extensions;

public static class EventStoreExtensions
{
    /// <summary>
    /// Register the RabbitMQ settings as EventBus
    /// </summary>
    /// <param name="services">BackgroundServices of DI</param>
    /// <param name="configuration">Configuration to get config</param>
    /// <param name="options">Options to overwrite default settings of Inbox and Outbox. </param>
    /// <param name="assemblies">Assemblies to find and load publisher and subscribers</param>
    public static void AddEventStore(this IServiceCollection services, IConfiguration configuration,
        Assembly[] assemblies,
        Action<InboxAndOutboxOptions> options = null)
    {
        var settingsType = typeof(InboxAndOutboxSettings);
        var isAlreadyRegistered = services.Any(serviceDescriptor =>
            serviceDescriptor.ServiceType == settingsType &&
            serviceDescriptor.Lifetime == ServiceLifetime.Singleton);
        //If it is already registered from another place, we need to just skeep it to avoid of registering it twice.
        if (isAlreadyRegistered)
            return;

        var settings = GetDefaultSettings();
        if (!settings.Inbox.IsEnabled && !settings.Outbox.IsEnabled)
            return;

        if (settings.Inbox.TableName == settings.Outbox.TableName)
            throw new ArgumentNullException("The table name for the index and outbox events cannot be the same.");

        services.AddSingleton(settings);

        if (settings.Outbox.IsEnabled)
        {
            services.AddScoped<IEventSenderManager, EventSenderManager>();
            services.AddScoped<IOutboxRepository>(serviceProvider =>
            {
                var _defaultSettings = serviceProvider.GetRequiredService<InboxAndOutboxSettings>();
                var _reporitory = new OutboxRepository(_defaultSettings.Outbox);

                return _reporitory;
            });

            RegisterAllEventsOfOutboxToDI(services, assemblies);
            services.AddSingleton<IEventsPublisherManager>(serviceProvider =>
            {
                var publisherManager = new EventsPublisherManager(serviceProvider);
                RegisterAllEventsOfOutbox(publisherManager, assemblies);

                return publisherManager;
            });

            services.AddHostedService<EventsPublisherService>();
            services.AddHostedService<CleanUpProcessedOutboxEventsService>();
        }

        if (settings.Inbox.IsEnabled)
        {
            services.AddScoped<IEventReceiverManager, EventReceiverManager>();
            services.AddScoped<IInboxRepository>(serviceProvider =>
            {
                var _defaultSettings = serviceProvider.GetRequiredService<InboxAndOutboxSettings>();
                var _reporitory = new InboxRepository(_defaultSettings.Inbox);

                return _reporitory;
            });

            RegisterAllEventsOfInboxToDI(services, assemblies);
            services.AddSingleton<IEventsReceiverManager>(serviceProvider =>
            {
                var receiverManager = new EventsReceiverManager(serviceProvider);
                RegisterAllEventsOfInbox(receiverManager, assemblies);

                return receiverManager;
            });

            services.AddHostedService<EventsReceiverService>();
            services.AddHostedService<CleanUpProcessedInboxEventsService>();
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

    private static void RegisterAllEventsOfOutbox(EventsPublisherManager publisherManager, Assembly[] assemblies)
    {
        var outboxEventTypes = GetPublisherHandlerTypes(assemblies);

        foreach (var (eventType, publisherType, provider) in outboxEventTypes)
            publisherManager.AddPublisher(eventType, publisherType, provider);
    }

    private static void RegisterAllEventsOfOutboxToDI(IServiceCollection services, Assembly[] assemblies)
    {
        var outboxEventTypes = GetPublisherHandlerTypes(assemblies);
        foreach (var (_, publisherType, _) in outboxEventTypes)
            services.AddTransient(publisherType);
    }

    static readonly Type PublishEventType = typeof(IEventPublisher<>);
    static readonly Type RabbitMqEventType = typeof(IRabbitMqEventPublisher<>);
    static readonly Type EmailEventType = typeof(IEmailEventPublisher<>);
    static readonly Type SmsEventType = typeof(ISmsEventPublisher<>);
    static readonly Type WebHookEventType = typeof(IWebHookEventPublisher<>);

    private static List<(Type eventType, Type publisherType, EventProviderType provider)> GetPublisherHandlerTypes(
        Assembly[] assemblies)
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
                        EventProviderType provider;
                        var genericType = implementedInterface.GetGenericTypeDefinition();
                        if (genericType == RabbitMqEventType)
                            provider = EventProviderType.RabbitMq;
                        else if (genericType == EmailEventType)
                            provider = EventProviderType.Email;
                        else if (genericType == SmsEventType)
                            provider = EventProviderType.Email;
                        else if (genericType == WebHookEventType)
                            provider = EventProviderType.WebHook;
                        else if (genericType == PublishEventType)
                            provider = EventProviderType.Unknown;
                        else
                            continue;

                        var eventType = implementedInterface.GetGenericArguments().Single();
                        subscriberHandlerTypes.Add((eventType, publisherType, provider));

                        break;
                    }
                }
            }
        }

        return subscriberHandlerTypes;
    }

    #endregion

    #region Receiver

    private static void RegisterAllEventsOfInbox(EventsReceiverManager receiverManager, Assembly[] assemblies)
    {
        var inboxEventTypes = GetReceiverHandlerTypes(assemblies);

        foreach (var (eventType, receiverType, provider) in inboxEventTypes)
            receiverManager.AddReceiver(eventType, receiverType, provider);
    }

    private static void RegisterAllEventsOfInboxToDI(IServiceCollection services, Assembly[] assemblies)
    {
        var inboxEventTypes = GetReceiverHandlerTypes(assemblies);
        foreach (var (_, receiverType, _) in inboxEventTypes)
            services.AddTransient(receiverType);
    }

    static readonly Type EventReceiveType = typeof(IEventReceiver<>);
    static readonly Type RabbitMqReceiveEventType = typeof(IRabbitMqEventReceiver<>);
    static readonly Type EmailReceiveEventType = typeof(IEmailEventReceiver<>);
    static readonly Type SmsReceiveEventType = typeof(ISmsEventReceiver<>);
    static readonly Type WebHookReceiveEventType = typeof(IWebHookEventReceiver<>);

    private static List<(Type eventType, Type receiverType, EventProviderType provider)> GetReceiverHandlerTypes(
        Assembly[] assemblies)
    {
        List<(Type eventType, Type receiverType, EventProviderType provider)> receiverHandlerTypes = new();
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
                        EventProviderType provider;
                        var genericType = implementedInterface.GetGenericTypeDefinition();
                        if (genericType == RabbitMqReceiveEventType)
                            provider = EventProviderType.RabbitMq;
                        else if (genericType == EmailReceiveEventType)
                            provider = EventProviderType.Email;
                        else if (genericType == SmsReceiveEventType)
                            provider = EventProviderType.Email;
                        else if (genericType == WebHookReceiveEventType)
                            provider = EventProviderType.WebHook;
                        else if (genericType == EventReceiveType)
                            provider = EventProviderType.Unknown;
                        else
                            continue;

                        var eventType = implementedInterface.GetGenericArguments().Single();
                        receiverHandlerTypes.Add((eventType, publisherType, provider));

                        break;
                    }
                }
            }
        }

        return receiverHandlerTypes;
    }

    #endregion
}