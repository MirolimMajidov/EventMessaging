using System.Reflection;
using EventStore.Inbox.Configurations;
using EventStore.Repositories.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        var inboxAndOutboxOptions = new InboxAndOutboxOptions(settings);
        options?.Invoke(inboxAndOutboxOptions);
        services.AddSingleton(settings);

        if (settings.Outbox.IsEnabled)
        {
            services.AddSingleton<IOutboxRepository>(serviceProvider =>
            {
                var _defaultSettings = serviceProvider.GetRequiredService<InboxAndOutboxSettings>();
                var _reporitory = new OutboxRepository(_defaultSettings.Outbox);

                return _reporitory;
            });
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

            return defaultSettings;
        }
    }
}