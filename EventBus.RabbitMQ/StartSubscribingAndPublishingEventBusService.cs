using System.Text;
using System.Text.Json;
using EventBus.RabbitMQ.Configurations;
using EventBus.RabbitMQ.Publishers;
using EventBus.RabbitMQ.Subscribers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMQ;

internal class StartSubscribingAndPublishingEventBusService : BackgroundService
{
    private readonly IEventSubscriberManager _subscriberManager;
    private readonly IEventPublisherManager _publisherManager;
    private readonly ILogger<StartSubscribingAndPublishingEventBusService> _logger;

    public StartSubscribingAndPublishingEventBusService(IEventSubscriberManager subscriberManager, IEventPublisherManager publisherManager, ILogger<StartSubscribingAndPublishingEventBusService> logger)
    {
        _publisherManager = publisherManager;
        _subscriberManager = subscriberManager;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _publisherManager.CreateExchangeForPublishers();
            _subscriberManager.CreateConsumerForEachQueueAndStartReceivingEvents();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while configuring publisher and subscriber of the RabbitMQ.");
        }
        
        return Task.CompletedTask;
    }
}