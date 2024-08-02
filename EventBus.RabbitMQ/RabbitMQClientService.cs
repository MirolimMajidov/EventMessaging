using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Publishers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ;

public class RabbitMQClientService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQClientService> _logger;

    public RabbitMQClientService(IOptions<RabbitMQOptions> options, ILogger<RabbitMQClientService> logger)
    {
        _logger = logger;
        try
        {
            _options = options.Value;
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

            _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while opening the RabbitMQ connection");
        }
    }

    public void Publish<T>(T message) where T : IEventPublisher
    {
        var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        _channel.BasicPublish(_options.ExchangeName, _options.RoutingKey, null, messageBody);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}