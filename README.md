## There are two libraries

### 1. EventBus.RabbitMQ
EventBus.RabbitMQ is a messaging library designed to simplify the implementation of communication using RabbitMQ. It enables seamless publishing and receiving of events between microservices or other types of applications. The library is easy to set up and is compatible with .NET8 or recent frameworks. Additionally, it supports working with multiple virtual hosts in RabbitMQ.

With this library, you can easily implement the [Inbox and outbox patterns](https://en.wikipedia.org/wiki/Inbox_and_outbox_pattern) in your application. It allows you to persist all incoming and outgoing event messages in the database. Currently, it supports storing event data only in a PostgreSQL database.

### NuGet package
[![Version](https://img.shields.io/nuget/v/Mirolim.EventBus.RabbitMQ?label=Version:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)

#### [See the EventBus.RabbitMQ documentation for more information](https://github.com/MirolimMajidov/EventMessaging?tab=readme-ov-file#getting-started-the-eventbusrabbitmq)

### 2. EventStorage
EventStorage is a library designed to simplify the implementation of the [Inbox and outbox patterns](https://en.wikipedia.org/wiki/Inbox_and_outbox_pattern) for handling multiple types of events in your application. It allows you to persist all incoming and outgoing event messages in the database. Currently, it supports storing event data only in a PostgreSQL database.

### NuGet package
[![Version](https://img.shields.io/nuget/v/Mirolim.EventStorage?label=Version:Mirolim.EventStorage)](https://www.nuget.org/packages/Mirolim.EventStorage)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventStorage?label=Downloads:Mirolim.EventStorage)](https://www.nuget.org/packages/Mirolim.EventStorage)

#### [See the EventStorage documentation for more information](https://github.com/MirolimMajidov/EventMessaging?tab=readme-ov-file#getting-started-the-eventstorage)

## Getting started the EventBus.RabbitMQ
EventBus.RabbitMQ is a messaging library designed to simplify the implementation of communication using RabbitMQ. It enables seamless publishing and receiving of events between microservices or other types of applications. The library is easy to set up and is compatible with .NET8 or recent frameworks. Additionally, it supports working with multiple virtual hosts in RabbitMQ.

With this library, you can easily implement the [Inbox and outbox patterns](https://en.wikipedia.org/wiki/Inbox_and_outbox_pattern) in your application. It allows you to persist all incoming and outgoing event messages in the database. Currently, it supports storing event data only in a PostgreSQL database.

### Setting up the library

Make sure you have installed and run [RabbitMQ](https://www.rabbitmq.com/docs/download) in your machine. After that, you need to install Mirolim.EventBus.RabbitMQ NuGet package.

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

To publish your event, you must first inject the `IEventPublisherManager` interface from the DI and pass your event object to the `Publish` method. Then, your event will be published.

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
        _logger.LogInformation("Id ({Id}): '{UserName}' user is created with the {UserId} id", @event.Id,
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
      "IsEnabled": true,
      "HostName": "localhost",
      "HostPort": 5672,
      "VirtualHost": "users",
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
        "VirtualHostKey": "users_test",
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
        "VirtualHostKey": "users_test",
        "QueueName": "payments_queue_UserService",
        "RoutingKey": "payments.created"
      }
    },
    "VirtualHostSettings": {
      "users_test": {
        "ExchangeName": "payments_exchange",
        "VirtualHost": "users/test",
        "QueueArguments": {
          "x-queue-type": "classic",
          "max-length-bytes": 1048576
        }
      },
      "payments": {
        "ExchangeName": "payments_exchange",
        "VirtualHost": "payments"
      }
    }
  }
```

A section may have the following subsections: <br/>
`DefaultSettings` - to set the default configuration/settings for connecting to the RabbitMQ and publishing and receiving messages. If you don't pass them, it will use default settings of RabbitMQ; The default settings has optional parameter named `QueueArguments` to pass the arguments to the queue. Another thing is that, by passing false to the `IsEnabled` option, we able to just disable using RabbitMQ.<br/>
`Publishers` - set custom settings for the publishers if needed. If you don't pass them, it will use the virtual host settings based on the `VirtualHostKey` which configured in the `VirtualHostSettings` section; <br/>
`Subscribers` - set custom settings for the subscribers if needed. If you don't pass them, it will use the virtual host settings based on the `VirtualHostKey` which configured in the `VirtualHostSettings` section; <br/>
`VirtualHostSettings` - adding virtual host configuration by given a key to use them from the publishers and subscribers. If we just add a new virtual host and not set all parameters, the not assigned properties automatically get/inherit a value from the default settings. If we don't want to use the default settings, we need to just set empty to the property to avoid auto-set. Then we can use the registered a virtual host from any subscribers or publishers by passing a `VirtualHostKey` value. <br/>

##### Can we use the TLS protocol while publishing events or subscribing to the events?
Yes, we can. For that we need to just enable the using the TLS protocol by adding the options below to the `DefaultSettings` if we want to use that in all events, or add them to the specific virtual host to use from the publishing or subscribing event:

```
"RabbitMQSettings": {
    "DefaultSettings": {
      //your settings
      "UseTls": true,
      "ClientCertPath": "path/to/client-cert.pem",
      "ClientKeyPath": "path/to/client-key.pem",
    },
    "Publishers": {
      "UserUpdated": {
          //your settings
        "VirtualHostKey": "users_test",
      }
    },
    "Subscribers": {
      "UserDeleted": {
          //your settings
        "VirtualHostKey": "payments",
      }
    },
    "VirtualHostSettings": {
      "users_test": {
        "ExchangeName": "payments_exchange",
        "VirtualHost": "users/test",
        "UseTls": false,
        "QueueArguments": {
          "x-queue-type": "classic",
          "max-length-bytes": 1048576
        }
      },
      "payments": {
        "ExchangeName": "payments_exchange",
        "VirtualHost": "payments",
        "UseTls": true,
        "ClientCertPath": "path/to/client-cert.pem",
        "ClientKeyPath": "path/to/client-key.pem",
      }
    }
  }
```

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
        "QueueName": "deleted_users_queue",
      }
    },
    "VirtualHostSettings": {
      "users": {
        "ExchangeName": "users_exchange",
        "QueueName": "users_queue",
      }
    }
  }
```

#### What if I want to subscribe to an event from another system that doesn't publish an event type?
When RabbitMQ receives an event from a `Consumer`, it tries to read the event type from the received event, if it can't find it, it uses the `routing key` instead to find the event subscriber.

### Advanced configuration of publishers and subscribers while registering to the DI services.

Since the library is designed to work with multiple a virtual hosts of RabbitMQ, there is a way to configure each publisher and subscriber separately from the configuration file or while registering to the DI services.
```
builder.Services.AddRabbitMQEventBus(builder.Configuration,
    assemblies: [typeof(Program).Assembly],
    defaultOptions: options =>
    {
        options.HostName = "localhost";
    },
    virtualHostSettingsOptions: settings =>
    {
        settings.Add("users_test", new RabbitMqHostSettings
        {
            HostName = "localhost",
            VirtualHost = "users/test",
            UserName = "admin",
            Password = "admin123",
            HostPort = 5672
        });
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
             op.VirtualHostKey = "users_test";
        });
    }
);
```

`defaultOptions` - it is an alternative way of overwriting `DefaultSettings` settings, to set the default configuration/settings for connecting to the RabbitMQ and publishing and receiving messages. If you don't pass them, it will use default settings of RabbitMQ; <br/>
`virtualHostSettingsOptions` - it is an alternative way of overwriting `VirtualHostSettings` settings, to register and overwrite settings of specific virtual host to use that from the subscribers and publishers if needed; <br/>
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

By default, while serializing and deserializing properties of event, it will use the `PascalCase`, but you can also use `CamelCase`, `SnakeCaseLower`, `SnakeCaseUpper`, `KebabCaseLower`, or `KebabCaseUpper` if you want. For this you need to add `PropertyNamingPolicy` option to `RabbitMQSettings` section if you want to apply it for all publishers or subscribers, or you can use/overwrite it from the specific a virtual host, or use/overwrite it from the publisher or subscriber event too. Example:
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
    },
    "VirtualHostSettings": {
      "users": {
        "PropertyNamingPolicy": "KebabCaseUpper"
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
The `InboxAndOutbox` is the main section for setting of the Outbox and Inbox functionalities. The `Outbox` and `Inbox` subsections offer numerous options. For a detailed explanation on using these options, go to the [options of Inbox and Outbox sections](https://github.com/MirolimMajidov/EventMessaging?tab=readme-ov-file#options-of-inbox-and-outbox-sections) of the EventStorage documentation.

Your application is now ready to use the Outbox feature. Now you can inject the `IEventSenderManager` interface from anywhere in your application, and use the `Send` method to publish your event.

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
        var succussfullySent = _eventSenderManager.Send(userCreated, EventProviderType.MessageBroker, eventPath);
        
        return Ok(item);
    }
}
```

Next, add an event publisher to manage a publishing event with the MessageBroker provider. Since the event storage functionality is designed as a separate library, it doesn't know about the actual sending of events. Therefore, we need to create single an event publisher to the specific provider, in our use case is for a MessageBroker.

```
public class MessageBrokerEventPublisher : IMessageBrokerEventPublisher
{
    private readonly IEventPublisherManager _eventPublisher;
    
    public MessageBrokerEventPublisher(IEventPublisherManager eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }
    
    public async Task<bool> Publish(ISendEvent @event, string eventPath)
    {
        _eventPublisher.Publish((IPublishEvent)@event);
        return await Task.FromResult(true);
    }
}
```
The MessageBrokerEventPublisher is serve for all kinds of events those are sending to the MessageBroker provider. But if you want to create event publisher for the event type for being able to use properties of event without casting, you need to just create event publisher by using generic interface of necessary publisher. In our use case is IMessageBrokerEventPublisher<UserCreated>.
```
public class CreatedUserMessageBrokerEventPublisher : IMessageBrokerEventPublisher<UserCreated>
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

Since we want to publish our an event to the RabbitMQ, the event subscriber must implement the `IMessageBrokerEventPublisher` by passing the type of event we want to publish. And, inject the `IEventPublisherManager` interface to publish the publishing `UserCreated` event to the `RabbitMQ`.
When we use the `Send` method of the `IEventSenderManager` to send an event, the event is first stored in the database. Based on our configuration (_by default, after one second_), the event will then be automatically execute the `Publish` method of created the `CreatedUserMessageBrokerEventPublisher` event publisher.

If an event fails for any reason, the server will automatically retry publishing it, with delays based on the configuration you set in the [Outbox section](https://github.com/MirolimMajidov/EventMessaging?tab=readme-ov-file#options-of-inbox-and-outbox-sections).

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
        //your Publishers
    },
    "Subscribers": {
        //your Subscribers
    },
    "VirtualHostSettings": {
        //your a virtual hosts settings
    }
  }
```

That's all. Now all incoming events from RabbitMQ are stored in the `Inbox` table of the database and then execute the `Receive` method of your event subscriber. See the [document of creating event subscriber](https://github.com/MirolimMajidov/EventMessaging?tab=readme-ov-file#create-a-subscriber-to-the-event).

#### Advanced configuration of the Inbox and Outbox functionalities while registering to the DI services.

Since the library is designed to  from multiple places, there is a way to configure the `Inbox` and `Outbox` functionalities from the configuration file or while registering to the DI services.
```
builder.Services.AddRabbitMQEventBus(builder.Configuration,
    assemblies: [typeof(Program).Assembly],
    defaultOptions: options =>
    {
        //Your settings
    },
    virtualHostSettingsOptions: settings =>
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
`eventStoreOptions` - it is an alternative way of overwriting configurations of the `Inbox` and `Outbox` functionalities. If you don't pass them, it will use default settings from the `AppSettings`. About other configurations, you can get information from [here](https://github.com/MirolimMajidov/EventMessaging?tab=readme-ov-file#advanced-configuration-of-publishers-and-subscribers-while-registering-to-the-di-services).


## Getting started the EventStorage

EventStorage is a library designed to simplify the implementation of the [Inbox and outbox patterns](https://en.wikipedia.org/wiki/Inbox_and_outbox_pattern) for handling multiple types of events in your application. It allows you to persist all incoming and outgoing event messages in the database. Currently, it supports storing event data only in a PostgreSQL database.

### NuGet package
[![Version](https://img.shields.io/nuget/v/Mirolim.EventStorage?label=Version:Mirolim.EventStorage)](https://www.nuget.org/packages/Mirolim.EventStorage)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventStorage?label=Downloads:Mirolim.EventStorage)](https://www.nuget.org/packages/Mirolim.EventStorage)


### Setting up the library

Make sure you have installed and run [PostgreSQL](https://www.postgresql.org/download/) in your machine. After that, you need to install Mirolim.EventStorage NuGet package.

```powershell
Install-Package Mirolim.EventStorage
```

Register the nuget package's necessary services to the services of DI in the Program.cs and pass the assemblies to find and load the events, publishers and receivers automatically:

```
builder.Services.AddEventStore(builder.Configuration,
    assemblies: [typeof(Program).Assembly]
    , options =>
    {
        options.Inbox.IsEnabled = true;
        options.Inbox.ConnectionString = "Connection string of the SQL database";
        //Other settings of the Inbox
        
        options.Outbox.IsEnabled = true;
        options.Outbox.ConnectionString = "Connection string of the SQL database";
        //Other settings of the Outbox
    });
```

Based on the configuration the tables will be automatically created while starting the server, if not exists.

### Using the Outbox pattern while publishing event
**Scenario 1:** _When user is deleted I need to notice the another service using the WebHook._<br/>

Start creating a structure of event to send. Your record must implement the `ISendEvent` interface. Example:

```
public record UserDeleted : ISendEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
}
```
The `Id` property is required, the other property can be added based on your business logic.<br/>

Since the library doesn't know about the actual sending of events, we need to create an event publisher specific to the type of event we want to publish. Add an event publisher by inheriting `IWebHookEventPublisher` and your `UserDeleted` event to manage a publishing event using the WebHook.

```
public class DeletedUserPublisher : IWebHookEventPublisher<UserDeleted>
{
    // private readonly IWebHookProvider _webHookProvider;
    //
    // public DeletedUserPublisher(IWebHookProvider webHookProvider)
    // {
    //     _webHookProvider = webHookProvider;
    // }

    public async Task<bool> Publish(UserDeleted @event, string eventPath)
    {
        //Add your logic
        return await Task.FromResult(false);
    }
}
```
The event provider support a few types: `MessageBroker`-for RabbitMQ message or any other message broker, `Sms`-for SMS message, `WebHook`- for WebHook call, `Email` for sending email, `Unknown` for other unknown type messages.
Depend on the event provider, the event subscriber must implement the necessary publisher interface: `IMessageBrokerEventPublisher`, `ISmsEventPublisher`, `IWebHookEventPublisher`, `IEmailEventPublisher` and `IUnknownEventPublisher`- for `Unknown` provider type.

Now you can inject the `IEventSenderManager` interface from anywhere in your application, and use the `Send` method to publish your event.

```
public class UserController : ControllerBase
{
    private readonly IEventSenderManager _eventSenderManager;

    public UserController(IEventSenderManager eventSenderManager)
    {
        _eventSenderManager = eventSenderManager;
    }
    
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        if (!Items.TryGetValue(id, out User item))
            return NotFound();

        var userDeleted = new UserDeleted { UserId = item.Id, UserName = item.Name };
        var webHookUrl = "https:example.com/api/users";
        var succussfullySent = _eventSenderManager.Send(userDeleted, EventProviderType.WebHook, webHookUrl);
        
        Items.Remove(id);
        return Ok(item);
    }
}
```

When we use the `Send` method of the `IEventSenderManager` to send an event, the event is first stored in the database. Based on our configuration (_by default, after one second_), the event will then be automatically execute the `Publish` method of created the `DeletedUserPublisher` event publisher.

If an event fails for any reason, the server will automatically retry publishing it, with delays based on the configuration you set in the [Outbox section](#options-of-inbox-and-outbox-sections).

**Scenario 2:** _When user is created I need to notice the another service using the RabbitMQ._<br/>

Start creating a structure of event to send. Your record must implement the `ISendEvent` interface. Example:

```
public record UserCreated : ISendEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
    
    public int Age { get; init; }
}
```

Next, add an event publisher to manage a publishing event with the MessageBroker provider. Since the event storage functionality is designed as a separate library, it doesn't know about the actual sending of events. Therefore, we need to create single an event publisher to the specific provider, in our use case is for a MessageBroker.

```
public class MessageBrokerEventPublisher : IMessageBrokerEventPublisher
{
    // private readonly IEventPublisherManager _eventPublisher;
    
    // public MessageBrokerEventPublisher(IEventPublisherManager eventPublisher)
    // {
    //     _eventPublisher = eventPublisher;
    // }
    
    public async Task<bool> Publish(ISendEvent @event, string eventPath)
    {
        // _eventPublisher.Publish((IPublishEvent)@event);
        return await Task.FromResult(true);
    }
}
```

The MessageBrokerEventPublisher is serve for all kinds of events, those are sending to the MessageBroker provider. But if you want to create event publisher for the event type for being able to use properties of event without casting, you need to just create event publisher by using generic interface of necessary publisher. In our use case is IMessageBrokerEventPublisher<UserCreated>.

```
public class CreatedUserMessageBrokerEventPublisher : IMessageBrokerEventPublisher<UserCreated>
{
    // private readonly IEventPublisherManager _eventPublisher;
    //
    // public CreatedUserMessageBrokerEventPublisher(IEventPublisherManager eventPublisher)
    // {
    //     _eventPublisher = eventPublisher;
    // }
    
    public async Task<bool> Publish(UserCreated @event, string eventPath)
    {
        // _eventPublisher.Publish(@event);
        //Add you logic to publish an event to the RabbitMQ
        
        return await Task.FromResult(true);
    }
}
```

Since we want to publish our an event to the RabbitMQ, the event subscriber must implement the `IMessageBrokerEventPublisher` by passing the type of event (`UserCreated`), we want to publish.
Your application is now ready to use this publisher. Inject the `IEventSenderManager` interface from anywhere in your application, and use the `Send` method to publish your `UserCreated` event.

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
        var routingKey = "usser.created";
        var succussfullySent = _eventSenderManager.Send(userCreated, EventProviderType.MessageBroker, routingKey);
        
        return Ok(item);
    }
}
```

##### Is there any way to add some additional data to the event while sending and use that while publishing event?

Yes, there is a way to do that. For that, we need to just implement `IHasAdditionalData` interface to the event structure of our sending event:

```
public record UserCreated : ISendEvent, IHasAdditionalData
{
    public Guid Id { get; }= Guid.NewGuid();
    
    public Guid UserId { get; init; } 
    
    public string UserName { get; init; }
    
    public Dictionary<string, string> AdditionalData { get; set; }
}
```

When we implement the implement `IHasAdditionalData` interface, it requires us to add collection property named `AdditionalData`. Now it is ready to use that:

```
var userCreated = new UserCreated { UserId = item.Id, UserName = item.Name };
userCreated.AdditionalData = new();
userCreated.AdditionalData.Add("login", "admin");
userCreated.AdditionalData.Add("password", "123");
var succussfullySent = _eventSenderManager.Send(userCreated, EventProviderType.MessageBroker, eventPath);
```

While publishing event, now you are able to read and use the added property from the your event:

```
public class CreatedUserMessageBrokerEventPublisher : IMessageBrokerEventPublisher<UserCreated>
{
    //Your logic
    
    public async Task<bool> Publish(UserCreated @event, string eventPath)
    {
        var login = @event.AdditionalData["login"];
        var password = @event.AdditionalData["password"];
        //Your logic
        _eventPublisher.Publish(@event);
        
        return await Task.FromResult(true);
    }
}
```

### Using the Inbox pattern while receiving event

Start creating a structure of event to receive. Your record must implement the `IReceiveEvent` interface. Example:

```
public record UserCreated : IReceiveEvent
{
    public Guid Id { get; init; }
    
    public Guid UserId { get; init; }
    
    public string UserName { get; init; }
    
    public int Age { get; init; }
}
```

Next, add an event receiver to manage a publishing RabbitMQ event.

```
public class UserCreatedReceiver : IRabbitMqEventReceiver<UserCreated>
{
    private readonly ILogger<UserCreatedReceiver> _logger;

    public UserCreatedReceiver(ILogger<UserCreatedReceiver> logger)
    {
        _logger = logger;
    }

    public async Task<bool> Receive(UserCreated @event)
    {
        _logger.LogInformation("Id ({Id}): {UserName} user is created with the {UserId} id", @event.Id,
            @event.UserName, @event.UserId);
        //Add your logic in here
        
        return await Task.FromResult(true);
    }
}
```

Now the `UserCreatedReceiver` receiver is ready to receive the event. To make it work, from your logic which you receive the event from the RabbitMQ, you need to inject the `IEventReceiverManager` interface and puss the received event to the `Received` method.

```
UserCreated receivedEvent = new UserCreated
{
    //Get created you data from the Consumer of RabbitMQ.
};
try
{
    IEventReceiverManager eventReceiverManager = scope.ServiceProvider.GetService<IEventReceiverManager>();
    if (eventReceiverManager is not null)
    {
        var succussfullyReceived = eventReceiverManager.Received(receivedEvent, eventArgs.RoutingKey, EventProviderType.RabbitMq);
        if(succussfullyReceived){
            //If the event received twice, it will return false. You need to add your logic to manage this use case.
        }
    }else{
        //the IEventReceiverManager will not be injected if the Inbox pattern is not enabled. You need to add your logic to manage this use case.
    }
}
catch (Exception ex)
{
    //You need to add logic to handle some unexpected use cases.
}
```

That's all. As we mentioned in above, the event provider support a few types: `MessageBroker`-for RabbitMQ message or any other message broker, `Sms`-for SMS message, `WebHook`- for WebHook call, `Email` for sending email, `Unknown` for other unknown type messages.
Depend on the event provider, the event receiver must implement the necessary receiver interface: `IMessageBrokerEventReceiver`, `ISmsEventReceiver`, `IWebHookEventReceiver`, `IEmailEventReceiver` and `IUnknownEventReceiver`- for `Unknown` provider type.

### Options of Inbox and Outbox sections

The `InboxAndOutbox` is the main section for setting of the Outbox and Inbox functionalities. The `Outbox` and `Inbox` subsections offer numerous options.

```
"InboxAndOutbox": {
    "Inbox": {
      //Your inbox settings
    },
    "Outbox": {
      "IsEnabled": false,
      "TableName": "Outbox",
      "MaxConcurrency": 10,
      "TryCount": 5,
      "TryAfterMinutes": 20,
      "TryAfterMinutesIfEventNotFound": 60,
      "SecondsToDelayProcessEvents": 2,
      "DaysToCleanUpEvents": 30,
      "HoursToDelayCleanUpEvents": 2,
      "ConnectionString": "Connection string of the SQL database"
    }
  }
```
**Description of options:**

`IsEnabled` - Enables or disables the use of Inbox/Outbox for storing received/sent events. Default value is false. <br/>
`TableName` - Specifies the table name used for storing received/sent events. Default value is "Inbox" for Inbox, "Outbox" for Outbox.<br/>
`MaxConcurrency` - Sets the maximum number of concurrent tasks for executing received/publishing events. Default value is 10.<br/>
`TryCount` - Defines the number of attempts before increasing the delay for the next retry. Default value is 10.<br/>
`TryAfterMinutes` - Specifies the number of minutes to wait before retrying if an event fails. Default value is 5.<br/>
`TryAfterMinutesIfEventNotFound` - For increasing the TryAfterAt to amount of minutes if the event not found to publish or receive. Default value is 60.<br/>
`SecondsToDelayProcessEvents` - The delay in seconds before processing events. Default value is 1.<br/>
`DaysToCleanUpEvents` - Number of days after which processed events are cleaned up. Cleanup only occurs if this value is 1 or higher. Default value is 0.<br/>
`HoursToDelayCleanUpEvents` - Specifies the delay in hours before cleaning up processed events. Default value is 1.<br/>
`ConnectionString` - The connection string for the PostgreSQL database used to store or read received/sent events.<br/>

All options of the Inbox and Outbox are optional, if we don't pass the value of them, it will use the default value of the option.
