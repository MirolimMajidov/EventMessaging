using EventBus.RabbitMQ;
using EventBus.RabbitMQ.Publishers;
using EventBus.RabbitMQ.Subscribers;
using Microsoft.AspNetCore.Mvc;

namespace PaymentsService.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly EventSubscriberManager _eventSubscriberManager;

    private readonly ILogger<PaymentController> _logger;

    public PaymentController(ILogger<PaymentController> logger, EventSubscriberManager eventSubscriberManager)
    {
        _logger = logger;
        _eventSubscriberManager = eventSubscriberManager;
    }

    [HttpGet]
    public IActionResult GetItems()
    {
        return Ok();
    }
}