using EventBus.RabbitMQ.Extensions;
using Payments.Service.Messaging.Handlers;
using PaymentsService.Messaging.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRabbitMQEventBus(builder.Configuration,
    options => { options.QueueName = "payments_queue"; },
    eventSubscriberManagerOptions: subscriberManager =>
    {
        subscriberManager.AddSubscriber<UserDeleted, UserDeletedHandler>(op => op.RoutingKey = "users.deleted");
    },
    assemblies: typeof(Program).Assembly);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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