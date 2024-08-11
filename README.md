# Getting Started

EventBus.RabbitMQ is an event messaging library that makes it easy to implement messaging communication with RabbitMQ to publish and receive events between microservice applications. It is easy to set up and runs on all recent .NET platforms and designed to work with the multiple a virtual hosts of RabbitMQ. 


## List of NuGet packages
[![Version](https://img.shields.io/nuget/vpre/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)


## Set up the library

Make sure you have installed and run [RabbitMQ](https://www.rabbitmq.com/docs/download) in your machine. After that, you need to install Mirolim.EventBus.RabbitMQ NuGet.

```powershell
Install-Package Mirolim.EventBus.RabbitMQ
```

Register the nuget package's necessary services to the services of DI in the Program.cs and pass the assemblies to find and load the publishers and subscribers automatically:

```
builder.Services.AddRabbitMQEventBus(builder.Configuration, assemblies: [typeof(Program).Assembly]);
```

## Create and publish an event massage

Start creating an event to publish. Your record must implement the `IPublishEvent` interface or inherit from the `PublishEvent` record. Example: 

```
public record UserDeleted : PublishEvent
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
        Items.Add(item.Id, item);

        var userCreated = new UserCreated { UserId = item.Id, UserName = item.Name };
        _eventPublisherManager.Publish(userCreated);
        
        return Ok(item);
    }
}
```

## Create a subscriber to the event

If you want to subscribe to necessary an event, first you need to create your own an event structure to subscribe. Your subscribe record must implement the `ISubscribeEvent` interface or inherit from the `SubscribeEvent` record. Example: 

```
public record UserCreated : SubscribeEvent
{
    public Guid UserId { get; set; }

    public string UserName { get; set; }
}
```

Then you need to create an event subscriber to receive an event. Your event subscriber class must implement the `IEventSubscriber<>` interface and implement your subscriber event structure. Example: 

```
public class UserCreatedSubscriber : IEventSubscriber<UserCreated>
{
    private readonly ILogger<UserCreatedSubscriber> _logger;

    public UserCreatedSubscriber(ILogger<UserCreatedSubscriber> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(UserCreated @event)
    {
        _logger.LogInformation("EventId ({EventId}): '{UserName}' user is created with the {UserId} id", @event.EventId,
            @event.UserName, @event.UserId);

        return await Task.FromResult(true);
    }
}
```

Depend on your business logic, you need to add your logic to the `Receive` method of subscriber to do something based on your received event.

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
      "ExchangeName": "users_exchange",
      "ExchangeType": "topic",
      "QueueName": "users_queue",
      "RoutingKey": "users.created",
      "RetryConnectionCount": 5,
      "QueueArguments": {
        "x-queue-type": "quorum"
      }
    },
    "Publishers": {
      "UserDeleted": {
        "VirtualHost": "users/test",
        "RoutingKey": "users.deleted",
        "PropertyNamingPolicy": "KebabCaseLower"
      },
      "UserUpdated": {
        "RoutingKey": "users.updated",
        "EventTypeName": "UserUpdatedEvent"
      }
    },
    "Subscribers": {
      "PaymentCreated": {
        "ExchangeName": "payments_exchange",
        "VirtualHost": "users/test",
        "RoutingKey": "payments.created",
        "QueueArguments": {
          "x-queue-type": "classic",
          "max-length-bytes": 1048576
        }
      }
    }
  },
```

A section may have the following subsections: <br/>
`DefaultSettings` - to set the default configuration/settings for connecting to the RabbitMQ and publishing and receiving messages. If you don't pass them, it will use default settings of RabbitMQ;  The default settings has optional parameter named `QueueArguments` to pass the arguments to the queue. <br/>
`Publishers` - set custom settings for the publishers if needed. If you don't pass them, it will use the default settings configured in the `DefaultSettings` section or RabbitMQ's default settings; <br/>
`Subscribers` - set custom settings for the subscribers if needed. If you don't pass them, it will use the default settings configured in the `DefaultSettings` section or RabbitMQ's default settings. The subscriber event has optional parameter named `QueueArguments` to pass the arguments to the queue.

### Customizing the event type of publishing/subscribing event
While publishing or subscribing an event by default it uses the Name of event structure. For example, if you add an event named `UserUpdated`, while publishing or subscribing/receiving that `UserUpdated` name as event type will be used. But if you want you can overwrite the event type by added event type name to the config file: 
```
"RabbitMQSettings": {
    "DefaultSettings": {
      //your settings
    },
    "Publishers": {
      "UserUpdated": {
        "RoutingKey": "users.updated",
        "EventTypeName": "UserUpdatedEvent"
      }
    },
    "Subscribers": {
      "UserDeleted": {
        //your settings
        "EventTypeName": "MyUserDeletedEvent"
      }
    }
  }
```

### What if I want to subscribe to an event from another system that doesn't publish an event type?
When RabbitMQ receives an event from a 'Consumer', it tries to read the event type from the received event, if it can't find it, it uses the 'routing key' instead to find the event subscriber.

## Advanced configuration of publishers and subscribers while registering to the DI services.

Since the library is designed to work with multiple a virtual hosts of RabbitMQ, there is a way to configure each publisher and subscriber separately from the configuration file or while registering to the DI services.
```
builder.Services.AddRabbitMQEventBus(builder.Configuration,
    assemblies: [typeof(Program).Assembly],
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
    }
);
```

`defaultOptions` - it is an alternative way of overwriting `DefaultSettings` settings, to set the default configuration/settings for connecting to the RabbitMQ and publishing and receiving messages. If you don't pass them, it will use default settings of RabbitMQ; <br/>
`eventPublisherManagerOptions` - it is an alternative way of overwriting `Publishers` settings, to register and set custom settings for the publishers if needed. If you don't pass them, it will use the default settings configured in the `DefaultSettings` section or RabbitMQ's default settings; <br/>
`eventSubscriberManagerOptions` - it is an alternative way of overwriting `Subscribers` settings, to register and set custom settings for the subscribers if needed. If you don't pass them, it will use the default settings configured in the `DefaultSettings` section or RabbitMQ's default settings; <br/>
`assemblies` - as I mentioned in above, it is to find and load the publishers and subscribers and register them to the services of DI automatically. It can be multiple assemblies depend on your design.

## Adding property to the publishing event's headers

Before publishing an event, you can attach properties to the event's headers by passing the header name and value to the `AddHeader` method. Keep in mind, the header name must be unique, otherwise it will throw exception. Example: 
```
var userUpdated = new UserUpdated { UserId = item.Id, OldUserName = item.Name, NewUserName = newName };
userUpdated.Headers = new();
userUpdated.Headers.Add("TraceId", HttpContext.TraceIdentifier);
_eventPublisherManager.Publish(userUpdated);
```

## Reading property from the subscribed event's headers

We can read the attached property value from the Headers collection of the received event. Example: 
```
public async Task<bool> Receive(UserCreated @event)
{
    if (@event.Headers?.TryGetValue("TraceId", out var traceId) == true)
    {
    }

    return await Task.FromResult(true);
}
```

## Changing a naming police for serializing and deserializing properties of Event

By default, while serializing and deserializing properties of event, it will use the `PascalCase`, but you can also use `CamelCase`, `SnakeCaseLower`, `SnakeCaseUpper`, `KebabCaseLower`, or `KebabCaseUpper` if you want. For this you need to add `PropertyNamingPolicy` option to `RabbitMQSettings` section if you want to apply it for all publishers or subscribers, or you can use it only for specific publisher or subscriber event. Example: 
```
"RabbitMQSettings": {
    "DefaultSettings": {
      //your settings
      "PropertyNamingPolicy": "KebabCaseLower"
    },
    "Publishers": {
      "PaymentCreated": {
        //your settings
        "PropertyNamingPolicy": "SnakeCaseUpper"
      }
    },
    "Subscribers": {
      "UserDeleted": {
        //your settings
        "PropertyNamingPolicy": "CamelCase"
      }
    }
  }
```
