using EventBus.RabbitMQ.Extensions;
using EventStore.Extensions;
using EventStore.Models.Outbox;
using UsersService.Messaging.Events;
using UsersService.Messaging.Events.Publishers;
using UsersService.Messaging.Events.Subscribers;
using UsersService.Messaging.Receivers;
using UsersService.Repositories;
using UsersService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRabbitMQEventBus(builder.Configuration,
    assemblies: [typeof(Program).Assembly],
    defaultOptions: options =>
    {
        options.HostName = "localhost";
        options.QueueArguments.Add("x-priority", 10);
    },
    eventPublisherManagerOptions: publisherManager =>
    {
        publisherManager.AddPublisher<UserDeleted>(op => op.RoutingKey = "users.deleted");
        publisherManager.AddPublisher<UserUpdated>(op => op.RoutingKey = "users.updated");
    },
    eventSubscriberManagerOptions: subscriberManager =>
    {
        subscriberManager.AddSubscriber<PaymentCreated, PaymentCreatedReceiver>(op =>
        {
            op.VirtualHost = "users/test";
        });
    }
);

builder.Services.AddEventStore(builder.Configuration,
    assemblies: [typeof(Program).Assembly]
    , options =>
    {
        options.Outbox.IsEnabled = true;
        options.Outbox.TableName = "SentEvents";
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IWebHookProvider, WebHookProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();