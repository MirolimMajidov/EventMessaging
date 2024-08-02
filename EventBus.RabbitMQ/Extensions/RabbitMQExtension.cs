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
        Action<EventPublisherManagerOptions>? eventPublisherManagerOptions = null, params Assembly[] assemblies)
    {
        var settings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
        var defaultSettings = GetDefaultRabbitMQOptions(settings);
        defaultOptions?.Invoke(defaultSettings);


        services.AddSingleton<IEventPublisherManager>(serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<EventPublisherManager>>();

            var publisherManager = new EventPublisherManager(defaultSettings, logger);
            var publishersOptions = settings?.Publishers ?? new Dictionary<string, RabbitMQEventOptions>();
            var allPublisherTypes = GetPublishers(assemblies);
            RegisterAllPublishers(publisherManager, allPublisherTypes, publishersOptions);

            var publisherManagerOptions = new EventPublisherManagerOptions(publisherManager);
            eventPublisherManagerOptions?.Invoke(publisherManagerOptions);

            publisherManager.CreateExchangeForPublishers();

            return publisherManager;
        });
        services.AddSingleton(defaultSettings);//TODO: does it needed
        services.AddSingleton<EventSubscriberManager>();

        //services.AddHostedService<RabbitMQConsumerService>();
        //TODO
    }

    private static Type[] GetPublishers(Assembly[]? assemblies)
    {
        var publisherType = typeof(IEventPublisher);
        if (assemblies is not null)
        {
            return assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && publisherType.IsAssignableFrom(t)).ToArray();
        }

        return [];
    }

    private static void RegisterAllPublishers(EventPublisherManager publisherManager,
        Type[] publisherTypes, Dictionary<string, RabbitMQEventOptions> publishersOptions)
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
}