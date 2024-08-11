using System.Reflection;
using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Publishers.Managers;
using EventBus.RabbitMQ.Publishers.Models;
using EventBus.RabbitMQ.Publishers.Options;
using EventBus.RabbitMQ.Subscribers.Managers;
using EventBus.RabbitMQ.Subscribers.Models;
using EventBus.RabbitMQ.Subscribers.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventBus.RabbitMQ.Extensions;

public static class RabbitMQExtensions
{
    /// <summary>
    /// Register the RabbitMQ settings as EventBus
    /// </summary>
    /// <param name="services">BackgroundServices of DI</param>
    /// <param name="configuration">Configuration to get config</param>
    /// <param name="defaultOptions">Default settings of RabbitMQ. It will overwrite all other default settings or settings those come from the configuration</param>
    /// <param name="eventPublisherManagerOptions">Options to register publisher with the settings. It will overwrite existing publisher setting if exists</param>
    /// <param name="eventSubscriberManagerOptions">Options to register subscriber with the settings. It will overwrite existing subscriber setting if exists</param>
    /// <param name="assemblies">Assemblies to find and load publisher and subscribers</param>
    public static void AddRabbitMQEventBus(this IServiceCollection services, IConfiguration configuration,
        Assembly[] assemblies,
        Action<RabbitMQOptions> defaultOptions = null,
        Action<EventPublisherManagerOptions> eventPublisherManagerOptions = null,
        Action<EventSubscriberManagerOptions> eventSubscriberManagerOptions = null)
    {
        var settings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
        var defaultSettings = GetDefaultRabbitMQOptions(settings);
        defaultOptions?.Invoke(defaultSettings);
        services.AddSingleton(defaultSettings);

        services.AddSingleton<IEventPublisherManager>(serviceProvider =>
        {
            var publisherManager = new EventPublisherManager(serviceProvider);

            var publishers = settings?.Publishers ?? new Dictionary<string, EventPublisherOptions>();
            var allPublisherTypes = GetPublisherTypes(assemblies);
            RegisterAllPublishers(publisherManager, allPublisherTypes, publishers);

            var publisherManagerOptions = new EventPublisherManagerOptions(publisherManager);
            eventPublisherManagerOptions?.Invoke(publisherManagerOptions);

            publisherManager.SetEventNameOfPublishers();

            return publisherManager;
        });

        RegisterAllSubscriberReceiversToDI(services, assemblies);

        services.AddSingleton<IEventSubscriberManager>(serviceProvider =>
        {
            var _defaultSettings = serviceProvider.GetRequiredService<RabbitMQOptions>();
            var subscriberManager = new EventSubscriberManager(_defaultSettings, serviceProvider);

            var subscribers = settings?.Subscribers ?? new Dictionary<string, EventSubscriberOptions>();
            RegisterAllSubscribers(subscriberManager, assemblies, subscribers);

            var subscriberManagerOptions = new EventSubscriberManagerOptions(subscriberManager);
            eventSubscriberManagerOptions?.Invoke(subscriberManagerOptions);

            subscriberManager.SetEventNameOfSubscribers();

            return subscriberManager;
        });

        services.AddHostedService<StartEventBusServices>();
    }

    #region Publishers

    static readonly Type PublisherType = typeof(IPublishEvent);
    private static Type[] GetPublisherTypes(Assembly[] assemblies)
    {
        if (assemblies is not null)
        {
            return assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false } && PublisherType.IsAssignableFrom(t)).ToArray();
        }

        return [];
    }

    private static void RegisterAllPublishers(EventPublisherManager publisherManager,
        Type[] publisherTypes, Dictionary<string, EventPublisherOptions> publishersOptions)
    {
        foreach (var typeOfPublisher in publisherTypes)
        {
            if (publishersOptions.TryGetValue(typeOfPublisher.Name, out var settings))
                publisherManager.AddPublisher(typeOfPublisher, settings);
            else
                publisherManager.AddPublisher(typeOfPublisher);
        }
    }

    private static RabbitMQOptions GetDefaultRabbitMQOptions(RabbitMQSettings settings)
    {
        var defaultSettings = RabbitMQOptionsConstant.CreateDefaultRabbitMQOptions();
        defaultSettings.OverwriteSettings(settings?.DefaultSettings);

        return defaultSettings;
    }

    #endregion

    #region Subscribers

    private static void RegisterAllSubscribers(EventSubscriberManager subscriberManager,
        Assembly[] assemblies, Dictionary<string, EventSubscriberOptions> subscribersOptions)
    {
        var subscriberReceiverTypes = GetSubscriberReceiverTypes(assemblies);

        foreach (var (eventType, handlerType) in subscriberReceiverTypes)
        {
            if (subscribersOptions.TryGetValue(eventType.Name, out var settings))
                subscriberManager.AddSubscriber(eventType, handlerType, settings);
            else
                subscriberManager.AddSubscriber(eventType, handlerType);
        }
    }

    private static void RegisterAllSubscriberReceiversToDI(IServiceCollection services, Assembly[] assemblies)
    {
        var subscriberReceiverTypes = GetSubscriberReceiverTypes(assemblies);
        foreach (var (_, handlerType) in subscriberReceiverTypes)
            services.AddTransient(handlerType);
    }

    static readonly Type PublisherReceiverType = typeof(IEventSubscriber<>);
    private static List<(Type eventType, Type receiverType)> GetSubscriberReceiverTypes(Assembly[] assemblies)
    {
        List<(Type eventType, Type receiverType)> subscriberHandlerTypes = new();
        if (assemblies is not null)
        {
            var allTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false });
            foreach (var type in allTypes)
            {
                foreach (var implementedInterface in type.GetInterfaces())
                {
                    if (implementedInterface.IsGenericType &&
                        implementedInterface.GetGenericTypeDefinition() == PublisherReceiverType)
                    {
                        var eventType = implementedInterface.GetGenericArguments().Single();
                        subscriberHandlerTypes.Add((eventType, type));
                        break;
                    }
                }
            }
        }

        return subscriberHandlerTypes;
    }

    #endregion
}