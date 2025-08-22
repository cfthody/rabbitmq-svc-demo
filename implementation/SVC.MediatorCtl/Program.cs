using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SVC.Core.Entities;
using SVC.SharedKernel;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.RegisterRabbitMqDependencies();

using var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>();
var log = logger.CreateLogger<Program>();

var rabbitmqConnection = host.Services.GetRequiredService<IRabbitMqConnection>();
await rabbitmqConnection.CreateConnectionAsync();

// const int maxOutstandingConfirms = 100;
// var channelOpts = new CreateChannelOptions(
//     publisherConfirmationsEnabled: true,
//     publisherConfirmationTrackingEnabled: true,
//     outstandingPublisherConfirmationsRateLimiter: new ThrottlingRateLimiter(maxOutstandingConfirms));

log.LogInformation("RabbitMQ Producer started...");
log.LogInformation("Type commands (e.g., 'create.employee 1') or 'exit' to quit:");

while (true)
{
    var command = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(command) || command.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        log.LogInformation("Exiting the application...");
        await rabbitmqConnection.PublishAsync<string>(
            "routing.event.close",
            ExchangeType.Fanout);

        break;
    }

    var parts = command.Split(' ');

    var routingKey = parts[0];
    var generateCount = parts[1];
    var publishCount = 1;
    if (parts.Length > 2)
    {
        int.TryParse(parts[2], out publishCount);
    }

    Entity? data = routingKey switch
    {
        "create.employee" => new Faker<Employee>()
            .RuleFor(e => e.Id, _ => 0)
            .RuleFor(e => e.Name, f => f.Person.FirstName)
            .RuleFor(e => e.EntryDate, f => f.Date.Past(1, DateTime.Now))
            .RuleFor(e => e.PlatoonId, f => 1)
            .RuleFor(e => e.RoleId, f => 1)
            .Generate(generateCount),
        "create.role" => new Faker<Role?>()
            .RuleFor(e => e.Id, _ => 0)
            .RuleFor(e => e.Name, f => f.Person.FirstName)
            .Generate(generateCount),
        "create.platoon" => new Faker<Platoon?>()
            .RuleFor(e => e.Id, _ => 0)
            .RuleFor(e => e.Name, f => f.Person.FirstName)
            .Generate(generateCount),
        _ => null
    };

    for (var i = 0; i < publishCount; i++)
    {
        try
        {
            await rabbitmqConnection.PublishAsync(
                "svc_message_bus",
                $"routing.event.{routingKey}",
                data);

            log.LogInformation($"Command '{routingKey}' has been published to the queue svc_message_queue.");
        }
        catch (Exception ex)
        {
            log.LogDebug($"An error occurred while publishing the message: {ex.Message}");
        }
    }
}

log.LogInformation("Application has exited.");
return;

async Task ConfirmPublishAsync()
{
    await using var channel = await rabbitmqConnection.Connection.CreateChannelAsync();

    var queueResult = await channel.QueueDeclareAsync();
}