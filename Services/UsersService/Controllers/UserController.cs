using EventBus.RabbitMQ.Publishers;
using EventStore.Models;
using EventStore.Models.Outbox;
using EventStore.Outbox;
using Microsoft.AspNetCore.Mvc;
using UsersService.Messaging.Events;
using UsersService.Messaging.Events.Publishers;
using UsersService.Models;
using IEventPublisherManager = EventBus.RabbitMQ.Publishers.IEventPublisherManager;

namespace UsersService.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IEventPublisherManager _eventPublisher;
    private readonly IEventSender _eventSender;

    private readonly ILogger<UserController> _logger;
    private static readonly Dictionary<Guid, User> Items = new();

    public UserController(ILogger<UserController> logger, IEventPublisherManager eventPublisher,
        IEventSender eventSender)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
        _eventSender = eventSender;
    }

    [HttpGet]
    public IActionResult GetItems()
    {
        return Ok(Items.Values);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetItems(Guid id)
    {
        if (!Items.TryGetValue(id, out User item))
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public IActionResult Create([FromBody] User item)
    {
        Items.Add(item.Id, item);

        var userCreated = new UserCreated { UserId = item.Id, UserName = item.Name };
        userCreated.Headers = new();
        userCreated.Headers.TryAdd("TraceId", HttpContext.TraceIdentifier);
        
        _eventSender.Send(userCreated, EventProviderType.RabbitMQ, userCreated.GetType().Name);
        return Ok();
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromQuery] string newName)
    {
        if (!Items.TryGetValue(id, out User item))
            return NotFound();

        var userUpdated = new UserUpdated { UserId = item.Id, OldUserName = item.Name, NewUserName = newName };
        userUpdated.Headers = new();
        userUpdated.Headers.TryAdd("TraceId", HttpContext.TraceIdentifier);
        _eventPublisher.Publish(userUpdated);

        item.Name = newName;
        return Ok(item);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        if (!Items.TryGetValue(id, out User item))
            return NotFound();

        var userDeleted = new UserDeleted { UserId = item.Id, UserName = item.Name };
        _eventSender.Send(userDeleted, EventProviderType.SMS, userDeleted.GetType().Name);
        
        Items.Remove(id);
        return Ok(item);
    }
}