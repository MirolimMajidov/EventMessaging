using System.Reflection;
using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Publishers;
using EventBus.RabbitMQ.Subscribers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventBus.RabbitMQ.Extensions;

public static class RabbitMQExtension
{
    /// <summary>
    /// Register the RabbitMQ settings as EventBus
    /// </summary>
    /// <param name="services">Services of DI</param>
    /// <param name="configuration">Configuration to get config</param>
    /// <param name="defaultOptions">Default settings of RabbitMQ. It will overwrite all other default settings or settings those come from the configuration</param>
    /// <param name="eventPublisherManagerOptions">Options to register publisher with the settings. It will overwrite existing publisher setting if exists</param>
    /// <param name="assemblies">Assemblies to find and load publisher and subscribers</param>
    public static void AddRabbitMQEventBus(this IServiceCollection services, IConfiguration configuration,
        Action<RabbitMQOptions>? defaultOptions = null,
        Action<EventPublisherManagerOptions>? eventPublisherManagerOptions = null,
        Action<EventSubscriberManagerOptions>? eventSubscriberManagerOptions = null,
        params Assembly[] assemblies)
    {
        var settings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
        var defaultSettings = GetDefaultRabbitMQOptions(settings);
        defaultOptions?.Invoke(defaultSettings);
        services.AddSingleton(defaultSettings);

        services.AddSingleton<IEventPublisherManager>(serviceProvider =>
        {
            var publisherManager = new EventPublisherManager(serviceProvider);
            var publisherManagerOptions = new EventPublisherManagerOptions(publisherManager);
            eventPublisherManagerOptions?.Invoke(publisherManagerOptions);

            var publishers = settings?.Publishers ?? new Dictionary<string, EventPublisherOptions>();
            var allPublisherTypes = GetPublisherTypes(assemblies);
            RegisterAllPublishers(publisherManager, allPublisherTypes, publishers);
            
            publisherManager.SetEventNameOfPublishers();
            publisherManager.CreateExchangeForPublishers();

            return publisherManager;
        });

        services.AddSingleton<EventSubscriberManager>(serviceProvider =>
        {
            var _defaultSettings = serviceProvider.GetRequiredService<RabbitMQOptions>();
            
            var subscriberManager = new EventSubscriberManager(_defaultSettings, serviceProvider);

            var subscriberManagerOptions = new EventSubscriberManagerOptions(subscriberManager);
            eventSubscriberManagerOptions?.Invoke(subscriberManagerOptions);

            var subscribers = settings?.Subscribers ?? new Dictionary<string, EventSubscriberOptions>();
            RegisterAllSubscribers(subscriberManager, assemblies, subscribers);

            subscriberManager.SetEventNameOfSubscribers();
            subscriberManager.CreateConsumerForEachQueue();

            return subscriberManager;
        });

        //services.AddHostedService<RabbitMQConsumerService>();
        //TODO
    }

    #region Publishers

    private static Type[] GetPublisherTypes(Assembly[]? assemblies)
    {
        var publisherType = typeof(IEventPublisher);
        if (assemblies is not null)
        {
            return assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t is { IsClass: true, IsAbstract: false } && publisherType.IsAssignableFrom(t)).ToArray();
        }

        return [];
    }

    private static void RegisterAllPublishers(EventPublisherManager publisherManager,
        Type[] publisherTypes, Dictionary<string, EventPublisherOptions> publishersOptions)
    {
        foreach (var publisherType in publisherTypes)
        {
            if (publishersOptions.TryGetValue(publisherType.Name, out var settings))
                publisherManager.AddPublisher(publisherType, settings);
            else
                publisherManager.AddPublisher(publisherType);
        }
    }

    private static RabbitMQOptions GetDefaultRabbitMQOptions(RabbitMQSettings? settings)
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
        var subscriberHandlerTypes = GetSubscriberHandlerTypes(assemblies);

        foreach (var (eventType, handlerType) in subscriberHandlerTypes)
        {
            if (subscribersOptions.TryGetValue(eventType.Name, out var settings))
                subscriberManager.AddSubscriber(eventType, handlerType, settings);
            else
                subscriberManager.AddSubscriber(eventType, handlerType);
        }
    }

    private static List<(Type eventType, Type handlerType)> GetSubscriberHandlerTypes(Assembly[]? assemblies)
    {
        List<(Type eventType, Type handlerType)> subscriberHandlerTypes = new();
        var publisherType = typeof(IEventSubscriberHandler<>);
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
                        implementedInterface.GetGenericTypeDefinition() == publisherType)
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