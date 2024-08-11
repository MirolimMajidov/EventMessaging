using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Connections;
using EventBus.RabbitMQ.Subscribers.Models;
using EventBus.RabbitMQ.Subscribers.Options;
using EventStore.Inbox.Managers;
using EventStore.Models;
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
    private readonly IRabbitMQConnection _connection;
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

        _connection = new RabbitMQConnection(_connectionOptions, serviceProvider);
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

        channel.ExchangeDeclare(exchange: _connectionOptions.ExchangeName, type: _connectionOptions.ExchangeType,
            durable: true, autoDelete: false);
        channel.QueueDeclare(_connectionOptions.QueueName, durable: true, exclusive: false, autoDelete: false,
            _connectionOptions.QueueArguments);
        foreach (var eventSettings in _subscribers.Values.Select(s => s.eventSettings))
            channel.QueueBind(_connectionOptions.QueueName, _connectionOptions.ExchangeName,
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
        try
        {
            if (_subscribers.TryGetValue(eventType,
                    out (Type eventType, Type eventHandlerType, EventSubscriberOptions eventSettings) info))
            {
                _logger.LogTrace("Received RabbitMQ event, Type is {EventType} and Id is {EventId}", eventType,
                    eventArgs.BasicProperties.MessageId);
                var jsonSerializerSetting = info.eventSettings.GetJsonSerializer();
                var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var receivedEvent =
                    JsonSerializer.Deserialize(message, info.eventType, jsonSerializerSetting) as ISubscribeEvent;
                LoadEventHeaders(receivedEvent);

                using var scope = _serviceProvider.CreateScope();
                if (_useInbox)
                {
                    IEventReceiverManager eventReceiverManager =
                        scope.ServiceProvider.GetService<IEventReceiverManager>();
                    if (eventReceiverManager is not null)
                    {
                        //TODO: Do we need to do something if it is not succussfully entered?
                        var succussfullyReceived = eventReceiverManager.Received(receivedEvent,
                            EventProviderType.RabbitMq, eventArgs.RoutingKey);
                        MarkEventIsDelivered();

                        return;
                    }

                    _logger.LogWarning(
                        "The RabbitMQ is configured to use the Inbox for received events, but the Inbox functionality of the EventStorage is not enabled. So, the {EventSubscriber} event subscriber of an event will be executed immediately for the event id: {EventId};",
                        info.eventHandlerType.Name, receivedEvent.EventId);
                }

                var eventHandlerSubscriber = scope.ServiceProvider.GetRequiredService(info.eventHandlerType);
                var handleMethod = info.eventHandlerType.GetMethod(HandlerMethodName);
                await (Task)handleMethod!.Invoke(eventHandlerSubscriber, [receivedEvent]);

                MarkEventIsDelivered();
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

        void MarkEventIsDelivered()
        {
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
        }

        void LoadEventHeaders(ISubscribeEvent eventSubscriber)
        {
            if (eventArgs.BasicProperties.Headers is not null)
            {
                eventSubscriber.Headers ??= new();
                foreach (var header in eventArgs.BasicProperties.Headers)
                {
                    var headerValue = Encoding.UTF8.GetString((byte[])header.Value);
                    eventSubscriber.Headers.Add(header.Key, headerValue);
                }
            }
        }
    }
}