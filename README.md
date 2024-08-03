# EventBus.RabbitMQ

EventBus.RabbitMQ is an event messaging library that makes it easy to implement messaging communication with RabbitMQ to publish and receive events between microservice applications. It is easy to set up and runs on all recent .NET platforms. 


## List of NuGet packages
[![Version](https://img.shields.io/nuget/vpre/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)
[![Downloads](https://img.shields.io/nuget/dt/Mirolim.EventBus.RabbitMQ?label=Downloads:Mirolim.EventBus.RabbitMQ)](https://www.nuget.org/packages/Mirolim.EventBus.RabbitMQ)


## Getting Started
### Setup the library

Make sure you have installed and runned [RabbitMQ](https://www.rabbitmq.com/docs/download) in your machine. After that, you need to instal Mirolim.EventBus.RabbitMQ NuGet.

```powershell
Install-Package Mirolim.EventBus.RabbitMQ
```

Register the nuget package's necessory services to the services of DI in the Program.cs and pass the assemblies to find and load the publishers and subscribers automatically:
```
builder.Services.AddRabbitMQEventBus(builder.Configuration, assemblies: typeof(Program).Assembly);
```

### Create and publish an event

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

### Create a subscriber with the handler and subscribe to the event

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
