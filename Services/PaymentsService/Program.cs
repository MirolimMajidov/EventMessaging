using EventBus.RabbitMQ.Extensions;
using PaymentsService.Messaging.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddRabbitMQEventBus(builder.Configuration,
    options =>
    {
    },
    publisherManager =>
    {
        publisherManager.AddPublisher<UserCreated>(op =>
        {
            op.RoutingKey = "UserCreated";
            op.RoutingKey = "users.*";
        });
    },
    typeof(Program).Assembly);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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