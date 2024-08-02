using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Publishers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly RabbitMQOptions _options;
    private IConnection _connection;
    private IModel _channel;

    private readonly ILogger<RabbitMQConsumerService> _logger;

    public RabbitMQConsumerService(IOptions<RabbitMQOptions> options, ILogger<RabbitMQConsumerService> logger)
    {
        _logger = logger;
        _options = options.Value;
        InitializeRabbitMqListener();
    }

    private void InitializeRabbitMqListener()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = (int)_options.HostPort!,
                VirtualHost = _options.VirtualHost,
                UserName = _options.UserName,
                Password = _options.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);
            
            // Declare and bind the queue to the Topic exchange
            _channel.QueueDeclare(_options.QueueName, durable:true, exclusive: false, autoDelete:false, null);
            _channel.QueueBind(_options.QueueName, _options.ExchangeName, _options.RoutingKey);
            //_channel.BasicQos(0, 1, false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while opening the RabbitMQ connection for consumer");
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var yourMessageModel = JsonSerializer.Deserialize<EventPublisher>(message);
            _logger.LogInformation("Message with the {id} id is received. Data is : {data}", yourMessageModel.EventId, message);
            // Handle your message

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(_options.QueueName, autoAck: false, consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}