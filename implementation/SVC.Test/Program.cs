// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;

using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

var builder = Host.CreateApplicationBuilder(args);
builder.AddRabbitMQClient("teste", config =>
{
});
var host = builder.Build();
await host.RunAsync();

var factory = new ConnectionFactory() 
{ 
    HostName = "localhost",
    UserName = "guest"
};


var conn = await factory.CreateConnectionAsync();
var channel = await conn.CreateChannelAsync();

// This message will only be published successfully if the user is "guest".
var properties = new BasicProperties
{
    UserId = "guest"
};

await channel.ExchangeDeclareAsync("linked-direct-exchange", ExchangeType.Topic);
await channel.ExchangeDeclareAsync("home-direct-exchange", ExchangeType.Topic);
await channel.ExchangeDeclareAsync("alternate-exchange", ExchangeType.Fanout, true);

// link exchanges
await channel.ExchangeBindAsync(
    "linked-direct-exchange",
    "home-direct-exchange",
    "homeAppliance");

await channel.QueueDeclareAsync("teste");


var arguments = new Dictionary<string, string>
{
    { "alternate-exchange", "alt.fanout.exchange" }
};
await channel.ExchangeDeclareAsync(
    "alt.topic.exchange", 
    ExchangeType.Topic, 
    true, 
    false, 
    arguments);


