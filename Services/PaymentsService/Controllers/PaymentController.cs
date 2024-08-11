using EventBus.RabbitMQ.Publishers.Managers;
using Microsoft.AspNetCore.Mvc;
using Payments.Service.Models;
using PaymentsService.Messaging.Events.Publishers;

namespace PaymentsService.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IEventPublisherManager _eventPublisherManager;

    private readonly ILogger<PaymentController> _logger;
    private static readonly Dictionary<Guid, Payment> Items = new();

    public PaymentController(ILogger<PaymentController> logger, IEventPublisherManager eventPublisherManagerManager)
    {
        _logger = logger;
        _eventPublisherManager = eventPublisherManagerManager;
    }

    [HttpGet]
    public IActionResult GetItems()
    {
        return Ok(Items.Values);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetItems(Guid id)
    {
        if (!Items.TryGetValue(id, out Payment item))
            return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public IActionResult Create([FromBody] Payment item)
    {
        Items.Add(item.Id, item);

        _eventPublisherManager.Publish(new PaymentCreated { PaymentId = item.Id, UserId = item.UserId, Amount = item.Amount });
        return Ok();
    }
}