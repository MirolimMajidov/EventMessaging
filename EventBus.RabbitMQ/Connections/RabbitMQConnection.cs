using System.Net.Sockets;
using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Subscribers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace EventBus.RabbitMQ.Connections;

internal class RabbitMQConnection : IRabbitMQConnection
{
    public bool IsConnected => _connection?.IsOpen == true && !_disposed;

    public int RetryConnectionCount { get; }

    private readonly IConnectionFactory _connectionFactory;
    private readonly BaseEventOptions _connectionOptions;
    private readonly ILogger<RabbitMQConnection> _logger;
    private IConnection _connection;
    private static string _connectTitle;

    public RabbitMQConnection(BaseEventOptions connectionOptions, IServiceProvider serviceProvider)
    {
        _connectionOptions = connectionOptions;
        _connectionFactory = new ConnectionFactory
        {
            HostName = connectionOptions.HostName,
            Port = (int)connectionOptions.HostPort!,
            VirtualHost = connectionOptions.VirtualHost,
            UserName = connectionOptions.UserName,
            Password = connectionOptions.Password,
            DispatchConsumersAsync = true
        };

        _logger = serviceProvider.GetRequiredService<ILogger<RabbitMQConnection>>();
        RetryConnectionCount = (int)connectionOptions.RetryConnectionCount!;

        string connectionDetail;
        if (connectionOptions is EventSubscriberOptions subscriberOptions)
            connectionDetail = $"'{subscriberOptions.QueueName}' queue of subscribers/receivers";
        else
            connectionDetail = $"'{connectionOptions.ExchangeName}' exchange of publishers";
        
        _connectTitle = $"The RabbitMQ connection is opened for the {connectionDetail} on the '{connectionOptions.HostName}' host's '{connectionOptions.VirtualHost}' virtual host.";
    }

    readonly object _lockOpenConnection = new();

    public bool TryConnect()
    {
        lock (_lockOpenConnection)
        {
            if (IsConnected) return true;

            _logger.LogTrace("RabbitMQ Client is trying to connect to the {VirtualHost} virtual host of {HostName} RabbitMQ host", _connectionOptions.VirtualHost, _connectionOptions.HostName);

            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(RetryConnectionCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex,
                            "RabbitMQ client could not connect to the {VirtualHost} virtual host of {HostName} RabbitMQ host after {TimeOut}s ({ExceptionMessage})",
                            _connectionOptions.VirtualHost, _connectionOptions.HostName, $"{time.TotalSeconds:n1}", ex.Message);
                    }
                );

            policy.Execute(() => { _connection = _connectionFactory.CreateConnection(); });

            if (IsConnected && _connection is not null)
            {
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;

                _logger.LogInformation(_connectTitle);

                return true;
            }

            _logger.LogCritical("FATAL ERROR: Connection to the {VirtualHost} virtual host of {HostName} RabbitMQ host could not be created and opened", _connectionOptions.VirtualHost, _connectionOptions.HostName);
            return false;
        }
    }

    void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
    {
        if (_disposed) return;

        TryConnect();
    }

    void OnCallbackException(object sender, CallbackExceptionEventArgs e)
    {
        if (_disposed) return;

        TryConnect();
    }

    void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
    {
        if (_disposed) return;

        TryConnect();
    }

    public IModel CreateChannel()
    {
        TryConnect();
        
        if (!IsConnected)
            throw new InvalidOperationException(
                $"RabbitMQ connection is not opened yet to the {_connectionOptions.VirtualHost} virtual host of {_connectionOptions.HostName}.");

        return _connection.CreateModel();
    }

    #region Dispose

    private bool _disposed;

    /// <summary>
    /// To close opened connection before disposing
    /// </summary>
    public void Dispose()
    {
        Disposing();
        GC.SuppressFinalize(this);
    }

    protected virtual void Disposing()
    {
        if (_disposed) return;

        try
        {
            _connection?.Dispose();
            _connection = null;

            _disposed = true;
        }
        catch (IOException ex)
        {
            _logger.LogCritical(ex.ToString());
        }
    }

    ~RabbitMQConnection()
    {
        Disposing();
    }

    #endregion
}