using EventBus.RabbitMQ.Publishers;
using EventStore.Models;
using EventStore.Models.Outbox;
using EventStore.Outbox;
using Microsoft.AspNetCore.Mvc;
using UsersService.Messaging.Events;
using UsersService.Models;

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
        userCreated.AdditionalData = new();
        userCreated.Headers.TryAdd("TraceId", HttpContext.TraceIdentifier);
        userCreated.Headers.TryAdd("TraceId2", HttpContext.TraceIdentifier);
        userCreated.AdditionalData.TryAdd("TraceId", HttpContext.TraceIdentifier);
        _eventSender.Send(userCreated, EventProviderType.RabbitMQ, userCreated.GetType().Name);
        _eventPublisher.Publish(userCreated);
        return Ok();
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update(Guid id, [FromQuery] string newName)
    {
        if (!Items.TryGetValue(id, out User item))
            return NotFound();

        var message = new UserUpdated { UserId = item.Id, OldUserName = item.Name, NewUserName = newName };
        item.Name = newName;

        _eventPublisher.Publish(message);

        return Ok(item);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        if (!Items.TryGetValue(id, out User item))
            return NotFound();

        _eventPublisher.Publish(new UserDeleted { UserId = item.Id, UserName = item.Name });
        Items.Remove(id);

        return Ok(item);
    }
}