using EventBus.RabbitMQ.Publishers;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IEventPublisherManager _eventPublisherManager;

    private readonly ILogger<OrderController> _logger;

    public OrderController(ILogger<OrderController> logger, IEventPublisherManager eventPublisherManager)
    {
        _logger = logger;
        _eventPublisherManager = eventPublisherManager;
    }

}