using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Connections;
using EventBus.RabbitMQ.Subscribers.Models;
using EventBus.RabbitMQ.Subscribers.Options;
using EventStorage.Inbox.Managers;
using EventStorage.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ.Subscribers.Consumers;

internal class EventConsumerService : IEventConsumerService
{
    private readonly bool _useInbox;
    private readonly EventSubscriberOptions _connectionOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventConsumerService> _logger;
    private readonly IRabbitMqConnection _connection;
    private IModel _consumerChannel;

    /// <summary>
    /// Dictionary collection to store all event and event handler information
    /// </summary>
    private readonly Dictionary<string, (Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings)>
        _subscribers = new();

    public EventConsumerService(EventSubscriberOptions connectionOptions, IServiceProvider serviceProvider,
        bool useInbox)
    {
        _connectionOptions = connectionOptions;
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetRequiredService<ILogger<EventConsumerService>>();

        _connection = new RabbitMqConnection(_connectionOptions, serviceProvider);
        _useInbox = useInbox;
    }

    public void AddSubscriber((Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings) eventInfo)
    {
        _subscribers.Add(eventInfo.eventSettings.EventTypeName!, eventInfo);
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

        channel.ExchangeDeclare(exchange: _connectionOptions.VirtualHostSettings.ExchangeName, type: _connectionOptions.VirtualHostSettings.ExchangeType,
            durable: true, autoDelete: false);
        channel.QueueDeclare(_connectionOptions.QueueName, durable: true, exclusive: false, autoDelete: false,
            _connectionOptions.VirtualHostSettings.QueueArguments);
        foreach (var eventSettings in _subscribers.Values.Select(s => s.eventSettings))
            channel.QueueBind(_connectionOptions.QueueName, _connectionOptions.VirtualHostSettings.ExchangeName,
                eventSettings.RoutingKey);

        channel.CallbackException += (_, ea) =>
        {
            _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartAndSubscribeReceiver();
        };

        return channel;
    }

    private const string HandlerMethodName = nameof(IEventSubscriber<ISubscribeEvent>.Receive);

    /// <summary>
    /// An event to receive all sent events
    /// </summary>
    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventType = eventArgs.BasicProperties.Type ?? eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
        try
        {
            if (_subscribers.TryGetValue(eventType,
                    out (Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings) info))
            {
                _logger.LogTrace("Received RabbitMQ event, Type is {EventType} and Id is {EventId}", eventType,
                    eventArgs.BasicProperties.MessageId);
                var jsonSerializerSetting = info.eventSettings.GetJsonSerializer();
                var receivedEvent =
                    JsonSerializer.Deserialize(message, info.eventType, jsonSerializerSetting) as ISubscribeEvent;
                var headers = GetEventHeaders();

                using var scope = _serviceProvider.CreateScope();
                if (_useInbox)
                {
                    IEventReceiverManager eventReceiverManager =
                        scope.ServiceProvider.GetService<IEventReceiverManager>();
                    if (eventReceiverManager is not null)
                    {
                        string headersAsJson = headers is null ? null : SerializeData(headers);
                        _ = eventReceiverManager.Received(receivedEvent, eventArgs.RoutingKey,
                            EventProviderType.MessageBroker, headersAsJson);
                        MarkEventIsDelivered();

                        return;
                    }

                    _logger.LogWarning(
                        "The RabbitMQ is configured to use the Inbox for received events, but the Inbox functionality of the EventStorage is not enabled. So, the {EventSubscriber} event subscriber of an event will be executed immediately for the event id: {EventId};",
                        info.eventHandlerType.Name, receivedEvent!.Id);
                }

                receivedEvent!.Headers = headers;
                var eventHandlerSubscriber = scope.ServiceProvider.GetRequiredService(info.eventHandlerType);
                var handleMethod = info.eventHandlerType.GetMethod(HandlerMethodName);
                await ((Task)handleMethod!.Invoke(eventHandlerSubscriber, [receivedEvent]))!;

                MarkEventIsDelivered();
            }
            else
            {
                _logger.LogWarning(
                    "No subscription for '{EventType}' event with the '{RoutingKey}' routing key and '{EventId}' event id. Event body: '{EventMessage}'",
                    eventType, eventArgs.RoutingKey, eventArgs.BasicProperties.MessageId, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "----- ERROR on receiving '{EventType}' event type with the '{RoutingKey}' routing key and '{EventId}' event id. Event body: '{EventMessage}'",
                eventType, eventArgs.RoutingKey, eventArgs.BasicProperties.MessageId, message);
        }

        void MarkEventIsDelivered()
        {
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }

        static string SerializeData<TValue>(TValue data)
        {
            return JsonSerializer.Serialize(data, data.GetType());
        }

        Dictionary<string, string> GetEventHeaders()
        {
            if (eventArgs.BasicProperties.Headers is not null)
            {
                var headers = new Dictionary<string, string>();
                foreach (var header in eventArgs.BasicProperties.Headers)
                {
                    var headerValue = Encoding.UTF8.GetString((byte[])header.Value);
                    headers.Add(header.Key, headerValue);
                }

                return headers;
            }

            return null;
        }
    }
}