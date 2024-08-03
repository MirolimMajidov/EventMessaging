using EventBus.RabbitMQ.Publishers;
using Microsoft.AspNetCore.Mvc;
using Payments.Service.Models;
using PaymentsService.Messaging.Events;

namespace PaymentsService.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IEventPublisherManager _eventPublisher;

    private readonly ILogger<PaymentController> _logger;
    private static readonly Dictionary<Guid, Payment> Items = new();

    public PaymentController(ILogger<PaymentController> logger, IEventPublisherManager eventPublisherManager)
    {
        _logger = logger;
        _eventPublisher = eventPublisherManager;
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

        _eventPublisher.Publish(new PaymentCreated { PaymentId = item.Id, UserId = item.UserId, Amount = item.Amount });
        return Ok();
    }
}