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
        var singlePublisherHandlerTypes = GetSinglePublisherHandlerTypes(assemblies);
        foreach (var (publisherType, provider) in singlePublisherHandlerTypes)
            publisherManager.AddGlobalPublisher(publisherType, provider);
        
        var outboxEventTypes = GetPublisherHandlerTypes(assemblies);

        foreach (var (eventType, publisherType, provider) in outboxEventTypes)
            publisherManager.AddPublisher(eventType, publisherType, provider);
    }

    private static void RegisterAllEventsOfOutboxToDI(IServiceCollection services, Assembly[] assemblies)
    {
        var singlePublisherHandlerTypes = GetSinglePublisherHandlerTypes(assemblies);
        foreach (var (publisherType, _) in singlePublisherHandlerTypes)
            services.AddTransient(publisherType);
        
        var outboxEventTypes = GetPublisherHandlerTypes(assemblies);
        foreach (var (_, publisherType, _) in outboxEventTypes)
            services.AddTransient(publisherType);
    }

    private static readonly Dictionary<Type, EventProviderType> EventPublisherTypesMap = new()
    {
        { typeof(IMessageBrokerEventPublisher<>), EventProviderType.MessageBroker },
        { typeof(ISmsEventPublisher<>), EventProviderType.Sms },
        { typeof(IEmailEventPublisher<>), EventProviderType.Email },
        { typeof(IWebHookEventPublisher<>), EventProviderType.WebHook },
        { typeof(IGrpcEventPublisher<>), EventProviderType.gRPC },
        { typeof(IUnknownEventPublisher<>), EventProviderType.Unknown }
    };

    private static List<(Type eventType, Type publisherType, EventProviderType provider)> GetPublisherHandlerTypes(Assembly[] assemblies)
    {
        //TOOD: I need to load all sendevent types to the memory and based on that, I need to find necessory publisher
        var subscriberHandlerTypes = new List<(Type eventType, Type publisherType, EventProviderType provider)>();

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

                        if (EventPublisherTypesMap.TryGetValue(genericType, out var provider))
                        {
                            var eventType = implementedInterface.GetGenericArguments().Single();
                            subscriberHandlerTypes.Add((eventType, publisherType, provider));
                            break;
                        }
                    }
                }
            }
        }

        return subscriberHandlerTypes;
    }
    
    private static readonly Dictionary<Type, EventProviderType> EventProviderTypeMap = new()
    {
        { typeof(IMessageBrokerEventPublisher), EventProviderType.MessageBroker },
        { typeof(ISmsEventPublisher), EventProviderType.Sms },
        { typeof(IEmailEventPublisher), EventProviderType.Email },
        { typeof(IWebHookEventPublisher), EventProviderType.WebHook },
        { typeof(IGrpcEventPublisher), EventProviderType.gRPC },
        { typeof(IUnknownEventPublisher), EventProviderType.Unknown }
    };

    private static List<(Type publisherType, EventProviderType provider)> GetSinglePublisherHandlerTypes(Assembly[] assemblies)
    {
        var subscriberHandlerTypes = new List<(Type publisherType, EventProviderType provider)>();

        if (assemblies != null)
        {
            var allTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract);

            foreach (var publisherType in allTypes)
            {
                foreach (var implementedInterface in publisherType.GetInterfaces())
                {
                    if (!implementedInterface.IsGenericType && EventProviderTypeMap.TryGetValue(implementedInterface, out var provider))
                    {
                        subscriberHandlerTypes.Add((publisherType, provider));
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

    private static readonly Dictionary<Type, EventProviderType> ReceiveEventTypesMap = new()
    {
        { typeof(IMessageBrokerEventReceiver<>), EventProviderType.MessageBroker },
        { typeof(IEmailEventReceiver<>), EventProviderType.Email },
        { typeof(ISmsEventReceiver<>), EventProviderType.Sms },
        { typeof(IWebHookEventReceiver<>), EventProviderType.WebHook },
        { typeof(IGrpcEventReceiver<>), EventProviderType.gRPC },
        { typeof(IUnknownEventReceiver<>), EventProviderType.Unknown }
    };

    private static List<(Type eventType, Type receiverType, EventProviderType provider)> GetReceiverHandlerTypes(Assembly[] assemblies)
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