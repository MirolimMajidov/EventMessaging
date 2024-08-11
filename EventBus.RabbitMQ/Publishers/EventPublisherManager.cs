using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Connections;
using EventBus.RabbitMQ.Publishers.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Publishers;

internal class EventPublisherManager : IEventPublisherManager
{
    private readonly RabbitMQOptions _defaultSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventPublisherManager> _logger;
    private readonly Dictionary<string, EventPublisherOptions> _publishers;
    private readonly Dictionary<string, IRabbitMQConnection> _openedRabbitMQConnections;

    public EventPublisherManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _defaultSettings = serviceProvider.GetRequiredService<RabbitMQOptions>();
        _logger = serviceProvider.GetRequiredService<ILogger<EventPublisherManager>>();
        _publishers = new();
        _openedRabbitMQConnections = new();
    }

    /// <summary>
    /// Registers a publisher.
    /// </summary>
    /// <param name="options">The options specific to the publisher, if any.</param>
    public void AddPublisher<TPublisher>(Action<EventPublisherOptions> options = null)
        where TPublisher : class, IPublishEvent
    {
        var publisherName = typeof(TPublisher).Name;
        if (_publishers.TryGetValue(publisherName, out var _settings))
        {
            options?.Invoke(_settings);
        }
        else
        {
            var settings = _defaultSettings.Clone<EventPublisherOptions>();
            options?.Invoke(settings);

            _publishers.Add(publisherName, settings);
        }
    }

    /// <summary>
    /// Registers a publisher.
    /// </summary>
    /// <param name="typeOfPublisher">The type of the publisher.</param>
    /// <param name="settings">The options specific to the publisher, if any.</param>
    public void AddPublisher(Type typeOfPublisher, EventPublisherOptions settings)
    {
        var publisherName = typeOfPublisher.Name;
        if (!_publishers.TryGetValue(publisherName, out var _settings))
        {
            _settings = _defaultSettings.Clone<EventPublisherOptions>();
            _publishers.Add(publisherName, _settings);
        }

        _settings.OverwriteSettings(settings);
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
    private IRabbitMQConnection CreateRabbitMQConnection(EventPublisherOptions settings)
    {
        var connectionId = $"{settings.VirtualHost}-{settings.ExchangeName}";
        if (!_openedRabbitMQConnections.TryGetValue(connectionId, out var connection))
        {
            connection = new RabbitMQConnection(settings, _serviceProvider);
            _openedRabbitMQConnections.Add(connectionId, connection);
        }

        return connection;
    }

    /// <summary>
    /// Creates RabbitMQ connection for the unique connection ID (VirtualHost+ExchangeName) and cache that.
    /// </summary>
    /// <param name="settings">Publisher setting to open connection</param>
    /// <returns>Create and return chanel after creating and opening RabbitMQ connection</returns>
    private IModel CreateRabbitMQChannel(EventPublisherOptions settings)
    {
        var connection = CreateRabbitMQConnection(settings);
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

                var channel = CreateRabbitMQChannel(eventSettings);
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

    private const string NameOfEventType = nameof(EventPublisherOptions.EventTypeName);

    public void Publish<TEventPublisher>(TEventPublisher @event) where TEventPublisher : IPublishEvent
    {
        try
        {
            var publisherName = @event.GetType().Name;
            var eventSettings = GetPublisherSettings(publisherName);
            using var channel = CreateRabbitMQChannel(eventSettings);

            var properties = channel.CreateBasicProperties();
            properties.MessageId = @event.EventId.ToString();
            properties.Type = eventSettings.EventTypeName;
            if (@event.Headers?.Any() == true)
            {
                var _headers = new Dictionary<string, object>();
                foreach (var item in  @event.Headers)
                    _headers.Add(item.Key, item.Value);
                properties.Headers =_headers;
            }

            var jsonSerializerSetting = eventSettings.GetJsonSerializer();
            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, jsonSerializerSetting));
            channel.BasicPublish(eventSettings.ExchangeName, eventSettings.RoutingKey, properties, messageBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while opening the RabbitMQ connection");
        }
    }
}