# Getting Started

### List of libraries

1. #### [EventBus.RabbitMQ: Event messaging library](#eventbus.rabbitmq)

1. #### [EventStorage: A library for implementing the Inbox and Outbox patterns](#eventstorage)

## EventBus.RabbitMQ
EventBus.RabbitMQ is a messaging library designed to simplify the implementation of communication using RabbitMQ. It enables seamless publishing and receiving of events between microservices or other types of applications. The library is easy to set up and is compatible with all recent .NET platforms. Additionally, it supports working with multiple virtual hosts in RabbitMQ.

With this library, you can easily implement the [Inbox and outbox patterns](https://en.wikipedia.org/wiki/Inbox_and_outbox_pattern) in your application. It allows you to persist all incoming and outgoing event messages in the database. Currently, it supports storing event data only in a PostgreSQL database.

### NuGet package
[![Version](https://img.shields.io/nuget/vpre/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)


### Setting up the library

Make sure you have installed and run [RabbitMQ](https://www.rabbitmq.com/docs/download) in your machine. After that, you need to install Mirolim.EventBus.RabbitMQ NuGet.

```powershell
Install-Package Mirolim.EventBus.RabbitMQ
```

Register the nuget package's necessary services to the services of DI in the Program.cs and pass the assemblies to find and load the publishers and subscribers automatically:

```
builder.Services.AddRabbitMQEventBus(builder.Configuration, assemblies: [typeof(Program).Assembly]);
```

### Create and publish an event massage

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

### Create a subscriber to the event

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

### Advanced configuration of publishers and subscribers from configuration file.

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

#### Customizing the event type of publishing/subscribing event:
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

#### What if I want to subscribe to an event from another system that doesn't publish an event type?
When RabbitMQ receives an event from a 'Consumer', it tries to read the event type from the received event, if it can't find it, it uses the 'routing key' instead to find the event subscriber.

### Advanced configuration of publishers and subscribers while registering to the DI services.

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

### Adding property to the publishing event's headers

Before publishing an event, you can attach properties to the event's headers by passing the header name and value to the `AddHeader` method. Keep in mind, the header name must be unique, otherwise it will throw exception. Example: 
```
var userUpdated = new UserUpdated { UserId = item.Id, OldUserName = item.Name, NewUserName = newName };
userUpdated.Headers = new();
userUpdated.Headers.Add("TraceId", HttpContext.TraceIdentifier);
_eventPublisherManager.Publish(userUpdated);
```

### Reading property from the subscribed event's headers

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

### Changing a naming police for serializing and deserializing properties of Event

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

### Setting up the Inbox and Outbox patterns in this library

As mentioned earlier, implementing the Inbox and Outbox patterns with this library is easy. Currently, it supports storing event data only in a PostgreSQL database.

#### How to use the Outbox pattern in this library?
As you know, the Outbox pattern for storing all outgoing events or messages of application in a database. To use this functionality, first you need to enable the `Outbox` feature by adding the following section to your AppSettings file.
```
"InboxAndOutbox": {
    "Inbox": {
      //Your inbox settings
    },
    "Outbox": {
      "IsEnabled": true,
      "ConnectionString": "Connection string of the SQL database"
      //...
    }
  }
```
The `InboxAndOutbox` is the main section for setting of the Outbox and Inbox functionalities. The `Outbox` and `Inbox` subsections offer numerous options. For a detailed explanation on using these options, go to the [options of Inbox and Outbox sections](#options-of-inbox-and-outbox-sections) of the EventStorage documentation.     

Your application is now ready to use the Outbox feature. Simply inject the `IEventSenderManager` interface from anywhere in your application, and use the `Send` method to publish your event.

```
public class UserController : ControllerBase
{
    private readonly IEventSenderManager _eventSenderManager;

    public UserController(IEventSenderManager eventSenderManager)
    {
        _eventSenderManager = eventSenderManager;
    }
    
    [HttpPost]
    public IActionResult Create([FromBody] User item)
    {
        Items.Add(item.Id, item);

        var userCreated = new UserCreated { UserId = item.Id, UserName = item.Name };
        //_eventPublisherManager.Publish(userCreated);
        
        var eventPath = userCreated.GetType().Name;
        var succussfullySent = _eventSenderManager.Send(userCreated, EventProviderType.RabbitMq, eventPath);
        
        return Ok(item);
    }
}
```

Next, add an event publisher to manage a publishing event. Since the event storage functionality is designed as a separate library, it doesn't know about the actual sending of events. Therefore, we'll need to create an event publisher specific to the type of event we want to publish.

```
public class CreatedUserPublisher : IRabbitMqEventPublisher<UserCreated>
{
    private readonly IEventPublisherManager _eventPublisher;

    public CreatedUserPublisher(IEventPublisherManager eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    
    public async Task<bool> Publish(UserCreated @event, string eventPath)
    {
        _eventPublisher.Publish(@event);
        
        return await Task.FromResult(true);
    }
}
```
Since we want to publish our an event to the RabbitMQ, the event subscriber must implement the `IRabbitMqEventPublisher` by passing the type of event we want to publish. And, inject the `IEventPublisherManager` interface to publish the publishing event to the `RabbitMQ`.
When we use the `Send` method of the IEventSenderManager to send an event, the event is first stored in the database. Based on our configuration (_by default, after one second_), the event will then be automatically execute the `Publish` method of created event publisher.

If an event fails for any reason, the server will automatically retry publishing it, with delays based on the configuration you set in the [Outbox section](#options-of-inbox-and-outbox-sections).

#### How to use the Inbox pattern in this library?

As you know, the Inbox pattern for storing all incoming events or messages to the application in a database. To use this functionality, first you need to enable the `Inbox` feature by adding the following section to your AppSettings file.
```
"InboxAndOutbox": {
    "Inbox": {
      "IsEnabled": true,
      "ConnectionString": "Connection string of the SQL database"
      //...
    },
    "Outbox": {
      //Your inbox settings
    }
  }
```

And then, set `true` to the `UseInbox` option of the `RabbitMQSettings.DefaultSettings`. Because by default it is disabled.

```
"RabbitMQSettings": {
    "DefaultSettings": {
        "UseInbox": true
        //your settings
    },
    "Publishers": {
        //your Subscribers
    },
    "Subscribers": {
        //your Subscribers
    }
  }
```

That's all. Now all incoming events from RabbitMQ are stored in the `Inbox` table of the database and then execute the `Receive` method of your event subscriber. See the [document of creating event subscriber](#create-a-subscriber-to-the-event).

#### Advanced configuration of the Inbox and Outbox functionalities while registering to the DI services.

Since the library is designed to  from multiple places, there is a way to configure the `Inbox` and `Outbox` functionalities from the configuration file or while registering to the DI services.
```
builder.Services.AddRabbitMQEventBus(builder.Configuration,
    assemblies: [typeof(Program).Assembly],
    defaultOptions: options =>
    {
        //Your settings
    },
    eventPublisherManagerOptions: publisherManager =>
    {
        //Your settings
    },
    eventSubscriberManagerOptions: subscriberManager =>
    {
        //Your settings
    },
    eventStoreOptions: options =>
    {
        options.Inbox.IsEnabled = true;
        options.Inbox.TableName = "ReceivedEvents";
        options.Outbox.IsEnabled = true;
        options.Outbox.TableName = "SentEvents";
    }
);
```
`eventStoreOptions` - it is an alternative way of overwriting configurations of the `Inbox` and `Outbox` functionalities. If you don't pass them, it will use default settings from the `AppSettings`. About other configurations, you can get information from [here](#advanced-configuration-of-publishers-and-subscribers-while-registering-to-the-di-services).


## EventStorage

Text Text

## Options of Inbox and Outbox sections