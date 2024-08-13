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
    private readonly RabbitMQOptions _defaultSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventPublisherManager> _logger;
    private readonly Dictionary<string, EventPublisherOptions> _publishers;
    private readonly Dictionary<string, IRabbitMQConnection> _openedRabbitMqConnections;

    public EventPublisherManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _defaultSettings = serviceProvider.GetRequiredService<RabbitMQOptions>();
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
            settings = _defaultSettings.Clone<EventPublisherOptions>();
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
        var publisherName = typeOfPublisher.Name;
        if (!_publishers.TryGetValue(publisherName, out var settings))
        {
            settings = _defaultSettings.Clone<EventPublisherOptions>();
            _publishers.Add(publisherName, settings);
        }

        settings.OverwriteSettings(publisherSettings);
    }

    /// <summary>
    /// Get setting of publisher by type.
    /// </summary>
    /// <param name="typeOfPublisher">The type of the publisher.</param>
    public void AddPublisher(Type typeOfPublisher)
    {
        AddPublisher(typeOfPublisher, _defaultSettings.Clone<EventPublisherOptions>());
    }

    /// <summary>
    /// Setting an event name of publisher if empty
    /// </summary>
    public void SetEventNameOfPublishers()
    {
        foreach (var (eventName, eventSettings) in _publishers)
        {
            if (string.IsNullOrEmpty(eventSettings.EventTypeName))
                eventSettings.EventTypeName = eventName;
        }
    }

    /// <summary>
    /// Creates RabbitMQ connection for the unique connection ID (VirtualHost+ExchangeName) and cache that.
    /// </summary>
    /// <param name="settings">Publisher setting to open connection</param>
    /// <returns>Returns create RabbitMQ connection</returns>
    private IRabbitMQConnection CreateRabbitMqConnection(EventPublisherOptions settings)
    {
        var connectionId = $"{settings.VirtualHost}-{settings.ExchangeName}";
        if (!_openedRabbitMqConnections.TryGetValue(connectionId, out var connection))
        {
            connection = new RabbitMQConnection(settings, _serviceProvider);
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
                var exchangeId = $"{eventSettings.VirtualHost}-{eventSettings.ExchangeName}";
                if (createdExchangeNames.Contains(exchangeId)) continue;

                var channel = CreateRabbitMqChannel(eventSettings);
                channel.ExchangeDeclare(eventSettings.ExchangeName, eventSettings.ExchangeType, durable: true,
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
                foreach (var item in  @event.Headers)
                    headers.Add(item.Key, item.Value);
                properties.Headers =headers;
            }

            var jsonSerializerSetting = eventSettings.GetJsonSerializer();
            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, publisherType, jsonSerializerSetting));
            channel.BasicPublish(eventSettings.ExchangeName, eventSettings.RoutingKey, properties, messageBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while opening the RabbitMQ connection");
        }
    }
}