using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ.Subscribers;

internal class EventConsumerService : IEventConsumerService
{
    private readonly EventSubscriberOptions _connectionOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumerService> _logger;
    private readonly IRabbitMQConnection _connection;
    private IModel _consumerChannel;

    /// <summary>
    /// Dictionary collection to store all event and event handler information
    /// </summary>
    private readonly Dictionary<string, (Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings)>
        _subscribers = new();

    public EventConsumerService(EventSubscriberOptions connectionOptions, IServiceProvider serviceProvider)
    {
        _connectionOptions = connectionOptions;
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILogger<EventConsumerService>>();

        _connection = new RabbitMQConnection(_connectionOptions, serviceProvider);
    }

    public void AddSubscriber((Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings) eventInfo)
    {
        _subscribers.Add(eventInfo.Item3.EventTypeName!, eventInfo);
    }

    /// <summary>
    /// Starts receiving events by creating a consumer
    /// </summary>
    public void StartAndSubscribeReceiver()
    {
        _consumerChannel = CreateConsumerChannel();
        var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
        consumer.Received += Consumer_Received;
        _consumerChannel.BasicConsume(queue: _connectionOptions.QueueName, autoAck: false, consumer: consumer);
    }

    /// <summary>
    /// To create channel for consumer
    /// </summary>
    /// <returns>Returns create channel</returns>
    private IModel CreateConsumerChannel()
    {
        _logger.LogTrace("Creating RabbitMQ consumer channel");

        var channel = _connection.CreateChannel();

        channel.ExchangeDeclare(exchange: _connectionOptions.ExchangeName, type: _connectionOptions.ExchangeType,
            durable: true, autoDelete: false);
        channel.QueueDeclare(_connectionOptions.QueueName, durable: true, exclusive: false, autoDelete: false,
            _connectionOptions.QueueArguments);
        foreach (var eventSettings in _subscribers.Values.Select(s => s.eventSettings))
            channel.QueueBind(_connectionOptions.QueueName, _connectionOptions.ExchangeName,
                eventSettings.RoutingKey);

        channel.CallbackException += (sender, ea) =>
        {
            _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartAndSubscribeReceiver();
        };

        return channel;
    }

    private const string HandlerMethodName = nameof(IEventSubscriberHandler<IEventSubscriber>.Handle);

    /// <summary>
    /// An event to receive all sent events
    /// </summary>
    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventType = eventArgs.BasicProperties.Type ?? eventArgs.RoutingKey;
        try
        {
            if (_subscribers.TryGetValue(eventType,
                    out (Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings) info))
            {
                _logger.LogTrace("Received RabbitMQ event, Type is {EventType} and Id is {EventId}", eventType,
                    eventArgs.BasicProperties.MessageId);
                using var scope = _serviceProvider.CreateScope();
                var jsonSerializerSetting = info.eventSettings.GetJsonSerializer();
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var eventSubscriber =
                    JsonSerializer.Deserialize(message, info.eventType, jsonSerializerSetting) as IEventSubscriber;
                var eventHandlerSubscriber = scope.ServiceProvider.GetRequiredService(info.eventHandlerType);
                LoadEventHeaders(eventSubscriber);

                var handleMethod = info.eventHandlerType.GetMethod(HandlerMethodName);
                await (Task)handleMethod.Invoke(eventHandlerSubscriber, [eventSubscriber]);

                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            else
            {
                _logger.LogWarning(
                    "No subscription for RabbitMQ {EventType} event with the {RoutingKey} routing key and {EventId} event id.",
                    eventType, eventArgs.RoutingKey, eventArgs.BasicProperties.MessageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "----- ERROR on receiving {EventType} event type with the {RoutingKey} routing key and {EventId} event id.",
                eventType, eventArgs.RoutingKey, eventArgs.BasicProperties.MessageId);
        }

        void LoadEventHeaders(IEventSubscriber eventSubscriber)
        {
            if (eventArgs.BasicProperties.Headers is not null)
            {
                foreach (var header in eventArgs.BasicProperties.Headers)
                {
                    var headerValue = Encoding.UTF8.GetString((byte[])header.Value);
                    eventSubscriber.Headers.Add(header.Key, headerValue);
                }
            }
        }
    }
}