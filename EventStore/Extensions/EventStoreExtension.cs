using System.Reflection;
using EventStore.Inbox.Configurations;
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
    public static void AddEventStore(this IServiceCollection services, IConfiguration configuration, Assembly[] assemblies,
        Action<InboxAndOutboxOptions> options = null)
    {
        var settings = configuration.GetSection(nameof(InboxAndOutbox)).Get<InboxAndOutbox>() ?? new();
        var inboxAndOutboxOptions = new InboxAndOutboxOptions(settings);
        options?.Invoke(inboxAndOutboxOptions);
        services.AddSingleton(settings);
    }
}