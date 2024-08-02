using EventBus.RabbitMQ;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly RabbitMQClientService _rabbitMqClientService;

    private readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger, RabbitMQClientService rabbitMqClientService)
    {
        _logger = logger;
        _rabbitMqClientService = rabbitMqClientService;
    }

}