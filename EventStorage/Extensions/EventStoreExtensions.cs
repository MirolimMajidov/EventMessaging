using System.Reflection;
using EventStorage.Configurations;
using EventStorage.Inbox;
using EventStorage.Inbox.BackgroundServices;
using EventStorage.Inbox.Managers;
using EventStorage.Inbox.Providers;
using EventStorage.Inbox.Repositories;
using EventStorage.Models;
using EventStorage.Outbox;
using EventStorage.Outbox.BackgroundServices;
using EventStorage.Outbox.Managers;
using EventStorage.Outbox.Models;
using EventStorage.Outbox.Providers;
using EventStorage.Outbox.Providers.EventProviders;
using EventStorage.Outbox.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventStorage.Extensions;

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
        var (globalPublisherHandlers, publisherHandlers) = GetPublisherHandlerTypes(assemblies);

        var allSendTypes = GetSendTypes(assemblies);
        foreach ((Type sendType, bool hasHeaders, bool hasAdditionalData) in allSendTypes)
        {
            foreach (var (publisherType, provider) in globalPublisherHandlers)
                publisherManager.AddPublisher(sendType, publisherType, provider, hasHeaders, hasAdditionalData,
                    isGlobalPublisher: true);

            if (publisherHandlers.TryGetValue(sendType.Name,
                    out (Type publisherType, EventProviderType provider) publisher))
                publisherManager.AddPublisher(sendType, publisher.publisherType, publisher.provider, hasHeaders,
                    hasAdditionalData, isGlobalPublisher: false);
        }
    }

    private static void RegisterAllEventsOfOutboxToDI(IServiceCollection services, Assembly[] assemblies)
    {
        var (globalPublisherHandlers, publisherHandlers) = GetPublisherHandlerTypes(assemblies);
        foreach (var (publisherType, _) in globalPublisherHandlers)
            services.AddTransient(publisherType);

        foreach (var publisher in publisherHandlers)
            services.AddTransient(publisher.Value.publisherType);
    }

    private static readonly Type SendEventType = typeof(ISendEvent);
    private static readonly Type HasHeadersType = typeof(IHasHeaders);
    private static readonly Type HasAdditionalDataType = typeof(IHasAdditionalData);

    private static List<(Type sendType, bool hasHeaders, bool hasAdditionalData)> GetSendTypes(
        Assembly[] assemblies)
    {
        var sentTypes = new List<(Type sendType, bool hasHeaders, bool hasAdditionalData)>();

        if (assemblies != null)
        {
            var allTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var type in allTypes)
            {
                if (SendEventType.IsAssignableFrom(type))
                {
                    var hasHeaders = HasHeadersType.IsAssignableFrom(type);
                    var hasAdditionalData = HasAdditionalDataType.IsAssignableFrom(type);
                    sentTypes.Add((type, hasHeaders, hasAdditionalData));
                }
            }
        }

        return sentTypes;
    }

    private static readonly Dictionary<Type, EventProviderType> GlobalEventProviderTypes = new()
    {
        { typeof(IMessageBrokerEventPublisher), EventProviderType.MessageBroker },
        { typeof(ISmsEventPublisher), EventProviderType.Sms },
        { typeof(IEmailEventPublisher), EventProviderType.Email },
        { typeof(IWebHookEventPublisher), EventProviderType.WebHook },
        { typeof(IGrpcEventPublisher), EventProviderType.gRPC },
        { typeof(IUnknownEventPublisher), EventProviderType.Unknown }
    };

    private static readonly Dictionary<Type, EventProviderType> EventProviderTypes = new()
    {
        { typeof(IMessageBrokerEventPublisher<>), EventProviderType.MessageBroker },
        { typeof(ISmsEventPublisher<>), EventProviderType.Sms },
        { typeof(IEmailEventPublisher<>), EventProviderType.Email },
        { typeof(IWebHookEventPublisher<>), EventProviderType.WebHook },
        { typeof(IGrpcEventPublisher<>), EventProviderType.gRPC },
        { typeof(IUnknownEventPublisher<>), EventProviderType.Unknown }
    };

    private static (List<(Type publisherType, EventProviderType provider)> globalPublisherHandlers,
        Dictionary<string, (Type publisherType, EventProviderType provider)> publisherHandlers)
        GetPublisherHandlerTypes(
            Assembly[] assemblies)
    {
        var globalPublisherHandlerTypes = new List<(Type publisherType, EventProviderType provider)>();
        var publisherHandlerTypes = new Dictionary<string, (Type publisherType, EventProviderType provider)>();

        if (assemblies != null)
        {
            var allTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var publisherType in allTypes)
            {
                foreach (var implementedInterface in publisherType.GetInterfaces())
                {
                    if (implementedInterface.IsGenericType)
                    {
                        var genericType = implementedInterface.GetGenericTypeDefinition();

                        if (EventProviderTypes.TryGetValue(genericType, out var provider))
                        {
                            var eventType = implementedInterface.GetGenericArguments().Single();
                            publisherHandlerTypes.Add(eventType.Name, (publisherType, provider));
                            break;
                        }
                    }
                    else if (GlobalEventProviderTypes.TryGetValue(implementedInterface, out var provider))
                    {
                        globalPublisherHandlerTypes.Add((publisherType, provider));
                    }
                }
            }
        }

        return (globalPublisherHandlerTypes, publisherHandlerTypes);
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

    private static readonly Dictionary<Type, EventProviderType> ReceiveEventTypesMap = new()
    {
        { typeof(IMessageBrokerEventReceiver<>), EventProviderType.MessageBroker },
        { typeof(IEmailEventReceiver<>), EventProviderType.Email },
        { typeof(ISmsEventReceiver<>), EventProviderType.Sms },
        { typeof(IWebHookEventReceiver<>), EventProviderType.WebHook },
        { typeof(IGrpcEventReceiver<>), EventProviderType.gRPC },
        { typeof(IUnknownEventReceiver<>), EventProviderType.Unknown }
    };

    private static List<(Type eventType, Type receiverType, EventProviderType provider)> GetReceiverHandlerTypes(
        Assembly[] assemblies)
    {
        var receiverHandlerTypes = new List<(Type eventType, Type receiverType, EventProviderType provider)>();

        if (assemblies != null)
        {
            var allTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var receiverType in allTypes)
            {
                foreach (var implementedInterface in receiverType.GetInterfaces())
                {
                    if (implementedInterface.IsGenericType)
                    {
                        var genericType = implementedInterface.GetGenericTypeDefinition();

                        if (ReceiveEventTypesMap.TryGetValue(genericType, out var provider))
                        {
                            var eventType = implementedInterface.GetGenericArguments().Single();
                            receiverHandlerTypes.Add((eventType, receiverType, provider));
                            break;
                        }
                    }
                }
            }
        }

        return receiverHandlerTypes;
    }

    #endregion
}