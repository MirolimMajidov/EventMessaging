using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Connections;
using EventBus.RabbitMQ.Publishers.Models;
using EventBus.RabbitMQ.Publishers.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Publishers.Managers;

internal class EventPublisherManager : IEventPublisherManager
{
    private readonly RabbitMqOptions _defaultSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventPublisherManager> _logger;
    private readonly Dictionary<string, EventPublisherOptions> _publishers;
    private readonly Dictionary<string, IRabbitMqConnection> _openedRabbitMqConnections;

    public EventPublisherManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _defaultSettings = serviceProvider.GetRequiredService<RabbitMqOptions>();
        _logger = serviceProvider.GetRequiredService<ILogger<EventPublisherManager>>();
        _publishers = new();
        _openedRabbitMqConnections = new();
    }

    /// <summary>
    /// Registers a publisher.
    /// </summary>
    /// <param name="options">The options specific to the publisher, if any.</param>
    public void AddPublisher<TPublisher>(Action<EventPublisherOptions> options = null)
        where TPublisher : class, IPublishEvent
    {
        var publisherName = typeof(TPublisher).Name;
        if (_publishers.TryGetValue(publisherName, out var settings))
        {
            options?.Invoke(settings);
        }
        else
        {
            settings = new EventPublisherOptions();
            options?.Invoke(settings);

            _publishers.Add(publisherName, settings);
        }
    }

    /// <summary>
    /// Registers a publisher.
    /// </summary>
    /// <param name="typeOfPublisher">The type of the publisher.</param>
    /// <param name="publisherSettings">The options specific to the publisher, if any.</param>
    public void AddPublisher(Type typeOfPublisher, EventPublisherOptions publisherSettings)
    {
        _publishers[typeOfPublisher.Name] = publisherSettings;
    }

    /// <summary>
    /// Setting the virtual host and other unassigned settings of publishers
    /// </summary>
    public void SetVirtualHostAndOwnSettingsOfPublishers(Dictionary<string, RabbitMqHostSettings> virtualHostsSettings)
    {
        foreach (var (eventTypeName, eventSettings) in _publishers)
        {
            var virtualHostSettings = string.IsNullOrEmpty(eventSettings.VirtualHostKey) ? _defaultSettings : virtualHostsSettings.GetValueOrDefault(eventSettings.VirtualHostKey, _defaultSettings);
            eventSettings.SetVirtualHostAndUnassignedSettings(virtualHostSettings, eventTypeName);
        }
    }

    /// <summary>
    /// Creates RabbitMQ connection for the unique connection ID (VirtualHost+ExchangeName) and cache that.
    /// </summary>
    /// <param name="settings">Publisher setting to open connection</param>
    /// <returns>Returns create RabbitMQ connection</returns>
    private IRabbitMqConnection CreateRabbitMqConnection(EventPublisherOptions settings)
    {
        var connectionId = $"{settings.VirtualHostSettings.VirtualHost}-{settings.VirtualHostSettings.ExchangeName}";
        if (!_openedRabbitMqConnections.TryGetValue(connectionId, out var connection))
        {
            connection = new RabbitMqConnection(settings, _serviceProvider);
            _openedRabbitMqConnections.Add(connectionId, connection);
        }

        return connection;
    }

    /// <summary>
    /// Creates RabbitMQ connection for the unique connection ID (VirtualHost+ExchangeName) and cache that.
    /// </summary>
    /// <param name="settings">Publisher setting to open connection</param>
    /// <returns>Create and return chanel after creating and opening RabbitMQ connection</returns>
    private IModel CreateRabbitMqChannel(EventPublisherOptions settings)
    {
        var connection = CreateRabbitMqConnection(settings);
        return connection.CreateChannel();
    }

    /// <summary>
    /// Creating an exchange for each registered publisher and 
    /// </summary>
    public void CreateExchangeForPublishers()
    {
        var createdExchangeNames = new List<string>();
        foreach (var (eventName, eventSettings) in _publishers)
        {
            try
            {
                var exchangeId =
                    $"{eventSettings.VirtualHostSettings.VirtualHost}-{eventSettings.VirtualHostSettings.ExchangeName}";
                if (createdExchangeNames.Contains(exchangeId)) continue;

                var channel = CreateRabbitMqChannel(eventSettings);
                channel.ExchangeDeclare(eventSettings.VirtualHostSettings.ExchangeName,
                    eventSettings.VirtualHostSettings.ExchangeType, durable: true,
                    autoDelete: false);

                createdExchangeNames.Add(exchangeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating an exchange for {publisherName} publisher.", eventName);
            }
        }

        createdExchangeNames.Clear();
    }

    private EventPublisherOptions GetPublisherSettings(string publisherName)
    {
        if (_publishers.TryGetValue(publisherName, out var settings))
            return settings;

        throw new KeyNotFoundException(
            $"The reading {publisherName} publisher does not exist in the registered publishers list.");
    }

    public void Publish<TEventPublisher>(TEventPublisher @event) where TEventPublisher : IPublishEvent
    {
        try
        {
            var publisherType = @event.GetType();
            var eventSettings = GetPublisherSettings(publisherType.Name);
            using var channel = CreateRabbitMqChannel(eventSettings);

            var properties = channel.CreateBasicProperties();
            properties.MessageId = @event.EventId.ToString();
            properties.Type = eventSettings.EventTypeName;
            if (@event.Headers?.Any() == true)
            {
                var headers = new Dictionary<string, object>();
                foreach (var item in @event.Headers)
                    headers.Add(item.Key, item.Value);
                properties.Headers = headers;
            }

            var jsonSerializerSetting = eventSettings.GetJsonSerializer();
            var messageBody =
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, publisherType, jsonSerializerSetting));
            channel.BasicPublish(eventSettings.VirtualHostSettings.ExchangeName, eventSettings.RoutingKey, properties,
                messageBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while opening the RabbitMQ connection");
        }
    }
}