// See https://aka.ms/new-console-template for more information

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SVC.SharedKernel;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.RegisterRabbitMqDependencies();

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>();
var log = logger.CreateLogger<Program>();

var rabbitmqConnection = host.Services.GetRequiredService<IRabbitMqConnection>();

await rabbitmqConnection.CreateConnectionAsync();

const string routingKey = "*.create.#";

var (channel, queueName) = await rabbitmqConnection.CreateReliableQueueAsync(
    exchange: "svc_message_bus", 
    type: ExchangeType.Topic,
    queueName: "proxyQueue");

await channel.QueueBindAsync(
    queue: queueName,
    exchange: "svc_message_bus",
    routingKey: routingKey);

using var httpClient = new HttpClient();
httpClient.BaseAddress = new Uri(" http://localhost:5285");

log.LogInformation("RabbitMQ API proxy Consumer started...");

var jsonContent = new StringContent(
    JsonSerializer.Serialize(new { }),
    Encoding.UTF8,
    "application/json");

log.LogInformation("Waiting for RabbitMQ messages to consume...");

await ConsumeRabbitMqMessageAsync();

while (true)
{
}

async Task ConsumeRabbitMqMessageAsync()
{
    var consumer = new AsyncEventingBasicConsumer(channel);
    consumer.ReceivedAsync += async (model, ea) =>
    {
        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
        switch (message)
        {
            case "create.employee":
                await httpClient.PostAsync("api/employees", jsonContent);
                break;

            case "create.role":
                await httpClient.PostAsync("api/roles", jsonContent);
                break;

            case "create.platoon":
                await httpClient.PostAsync("api/platoons", jsonContent);
                break;
        }

        log.LogInformation($"Received {message}");
        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, false);
    };

    await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
}