using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Configurations;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Publishers;

internal class EventPublisherManager(RabbitMQOptions defaultSettings, ILogger<EventPublisherManager> logger)
    : IEventPublisherManager
{
    private readonly Dictionary<string, EventPublisherOptions> _publishers = new();

    /// <summary>
    /// Registers a publisher.
    /// </summary>
    /// <param name="options">The options specific to the publisher, if any.</param>
    public void AddPublisher<TPublisher>(Action<EventPublisherOptions>? options = null)
        where TPublisher : class, IEventPublisher
    {
        var publisherName = typeof(TPublisher).Name;
        if (_publishers.TryGetValue(publisherName, out var _settings))
        {
            options?.Invoke(_settings);
        }
        else
        {
            var settings  = defaultSettings.Clone<EventPublisherOptions>();
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
            _settings  = defaultSettings.Clone<EventPublisherOptions>();
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
        AddPublisher(typeOfPublisher, defaultSettings.Clone<EventPublisherOptions>());
    }

    /// <summary>
    /// Creating an exchange for each registered publisher
    /// </summary>
    public void CreateExchangeForPublishers()
    {
        foreach (var publisher in _publishers)
        {
            try
            {
                var channel = CreateChannel(publisher.Key, out var eventSettings);
                if (channel is null)
                {
                    logger.LogWarning(
                        "An exchange did not create for {publisherName} publisher with the {exchangeName}. Something is wrong while opening connection to the RabbitMQ.",
                        publisher.Key, eventSettings!.ExchangeName);
                    continue;
                }

                channel.ExchangeDeclare(eventSettings!.ExchangeName, eventSettings.ExchangeType, durable: true, autoDelete: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while creating an exchange for {publisherName} publisher.", publisher.Key);
            }
        }
    }

    private EventPublisherOptions GetPublisherSettings(string publisherName)
    {
        if (_publishers.TryGetValue(publisherName, out var settings))
            return settings;

        throw new KeyNotFoundException(
            $"The reading {publisherName} publisher does not exist in the registered publishers list.");
    }

    public void Publish<TEventPublisher>(TEventPublisher @event) where TEventPublisher : IEventPublisher
    {
        try
        {
            var publisherName = @event.GetType().Name;
            var channel = CreateChannel(publisherName, out EventPublisherOptions? eventSettings);
            if (channel is null)
            {
                logger.LogWarning(
                    "The {publisherName} publisher could not sent. Something is wrong while finding a publisher settings or opening connection to the RabbitMQ.",
                    publisherName);
                return;
            }

            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));
            channel.BasicPublish(eventSettings!.ExchangeName, eventSettings.RoutingKey, null, messageBody);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while opening the RabbitMQ connection");
        }
    }

    private IModel? CreateChannel(string publisherType, out EventPublisherOptions? settings)
    {
        settings = null;
        try
        {
            settings = GetPublisherSettings(publisherType);
            var factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = (int)settings.HostPort!,
                VirtualHost = settings.VirtualHost,
                UserName = settings.UserName,
                Password = settings.Password,
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            return connection.CreateModel();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while opening the RabbitMQ connection.");
            return null;
        }
    }
}