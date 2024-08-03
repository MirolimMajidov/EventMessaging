# Getting Started

EventBus.RabbitMQ is an event messaging library that makes it easy to implement messaging communication with RabbitMQ to publish and receive events between microservice applications. It is easy to set up and runs on all recent .NET platforms and designed to work with the multible a virtual hosts of RabbitMQ. 


## List of NuGet packages
[![Version](https://img.shields.io/nuget/vpre/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)


## Setup the library

Make sure you have installed and runned [RabbitMQ](https://www.rabbitmq.com/docs/download) in your machine. After that, you need to instal Mirolim.EventBus.RabbitMQ NuGet.

```powershell
Install-Package Mirolim.EventBus.RabbitMQ
```

Register the nuget package's necessory services to the services of DI in the Program.cs and pass the assemblies to find and load the publishers and subscribers automatically:
```
builder.Services.AddRabbitMQEventBus(builder.Configuration, assemblies: typeof(Program).Assembly);
```

## Create and publish an event

Start creating an event to publish. Your class must implement the `IEventPublisher` interface or inherit from the `EventPublisher` class. Example: 
```
public class UserDeleted : EventPublisher
{
    public Guid UserId { get; set; }
    
    public string UserName { get; set; }
}
```
In publish your event, you must first inject the `IEventPublisherManager` interface from the DI and pass your event object to the `Publish` method. Then, your event will be published.
```
public class UserController : ControllerBase
{
    private readonly IEventPublisherManager _eventPublisherManager;

    public UserController(IEventPublisherManager eventPublisherManager)
    {
        _eventPublisherManager = eventPublisherManager;
    }
    
    [HttpPost]
    public IActionResult Create([FromBody] User item)
    {
        _eventPublisherManager.Publish(new UserCreated { UserId = item.Id, UserName = item.Name });
        return Ok();
    }
}
```

## Create a subscriber with the handler and subscribe to the event

If you want to subscribe to necessory event, first you need to create your own an event structure to subscribe. Your subscriber class must implement the `IEventSubscriber` interface or inherit from the `EventSubscriber` class. Example: 
```
public class UserCreated : EventSubscriber
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}
```
Then you need to create a subscriber handler to receive the event. Your subscriber handler class must implement the `IEventSubscriberHandler<>` interface and impmenet your subsciber class. Example: 
```
public class UserCreatedHandler : IEventSubscriberHandler<UserCreated>
{
    private readonly ILogger<UserCreatedHandler> _logger;

    public UserCreatedHandler(ILogger<UserCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreated @event)
    {
        _logger.LogInformation("EventId ({EventId}): {UserName} user is created with the {UserId} id", @event.EventId,
            @event.UserName, @event.UserId);

        return Task.CompletedTask;
    }
}
```
Depend on your business logic, you need to add your logic to the `Handle` method of handler to do something based on your received event.

## Advanced configuration of publishers and subscribers from configuration file.

First you need to add a new section called `RabbitMQSettings` to your configuration file.

```
"RabbitMQSettings": {
    "DefaultSettings": {
      "HostName": "localhost",
      "HostPort": 5672,
      "VirtualHost": "users/pro",
      "UserName": "admin",
      "Password": "admin123",
      "ExchangeName": "users_exchange_name",
      "ExchangeType": "topic",
      "QueueName": "users_queue",
      "RoutingKey": "users.created",
      "RetryConnectionCount": 5
    },
    "Publishers": {
      "UserDeleted": {
        "VirtualHost": "users/test"
      },
      "UserUpdated": {
        "EventTypeName": "UserUpdatedEvent"
      }
    },
    "Subscribers": {
      "PaymentCreated": {
        "ExchangeName": "payments_exchange_name",
        "VirtualHost": "users/test",
        "RoutingKey": "payments.created"
      }
    }
  }
```

A section may have the following subsections:
`DefaultSettings` - to set the default configuration/settings for connecting to the RabbintMQ and publishing and receiving messages. If you don't pass them, it will use default settings of RabbitMQ;
`Publishers` - set custom settings for the publishers if needed. If you don't pass them, it will use the default settings configured in the `DefaultSettings` section or RabbitMQ's default settings;
`Subscribers` - set custom settings for the subscibers if needed. If you don't pass them, it will use the default settings configured in the `DefaultSettings` section or RabbitMQ's default settings.

## Advanced configuration of publishers and subscribers while registring to the DI services.

Since the library is designed to work with multiple virtual hosts of RabbitMQ, there is a way to configure each publisher and subscriber separately from the configuration file or while registring to the DI services.
```
builder.Services.AddRabbitMQEventBus(builder.Configuration,
    defaultOptions: options =>
    {
        options.HostName = "localhost";
    },
    eventPublisherManagerOptions: publisherManager =>
    {
        publisherManager.AddPublisher<UserDeleted>(op => op.RoutingKey = "users.deleted");
        publisherManager.AddPublisher<UserUpdated>(op => op.RoutingKey = "users.updated");
    },
    eventSubscriberManagerOptions: subscriberManager =>
    {
        subscriberManager.AddSubscriber<PaymentCreated, PaymentCreatedHandler>(op =>
        {
            op.VirtualHost = "users/test";
        });
    },
    assemblies: typeof(Program).Assembly
);
```

`defaultOptions` - it is alternative way of overwriting `DefaultSettings` settings, to set the default configuration/settings for connecting to the RabbintMQ and publishing and receiving messages. If you don't pass them, it will use default settings of RabbitMQ;
`eventPublisherManagerOptions` - it is alternative way of overwriting `Publishers` settings, to registir and set custom settings for the publishers if needed. If you don't pass them, it will use the default settings configured in the `DefaultSettings` section or RabbitMQ's default settings;
`eventSubscriberManagerOptions` - it is alternative way of overwriting `Subscribers` settings, to registir and set custom settings for the subscibers if needed. If you don't pass them, it will use the default settings configured in the `DefaultSettings` section or RabbitMQ's default settings;
`assemblies` - as I mentioned in above, it is to find and load the publishers and subscribers and register them to the services of DI automatically. It can be multiple assemblies depend on your design.

