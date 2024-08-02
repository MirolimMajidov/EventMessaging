using EventBus.RabbitMQ;
using EventBus.RabbitMQ.Publishers;
using Microsoft.AspNetCore.Mvc;

namespace PaymentsService.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IEventPublisherManager _eventPublisherManager;

    private readonly ILogger<PaymentController> _logger;

    public PaymentController(ILogger<PaymentController> logger, IEventPublisherManager eventPublisherManager)
    {
        _logger = logger;
        _eventPublisherManager = eventPublisherManager;
    }

}