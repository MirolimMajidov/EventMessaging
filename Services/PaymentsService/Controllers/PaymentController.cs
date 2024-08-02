using EventBus.RabbitMQ;
using Microsoft.AspNetCore.Mvc;

namespace PaymentsService.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly RabbitMQClientService _rabbitMqClientService;

    private readonly ILogger<PaymentController> _logger;

    public PaymentController(ILogger<PaymentController> logger, RabbitMQClientService rabbitMqClientService)
    {
        _logger = logger;
        _rabbitMqClientService = rabbitMqClientService;
    }

}