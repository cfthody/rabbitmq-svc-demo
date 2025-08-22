using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace SVC.SharedKernel;

public interface IRabbitMqConnection
{
    IConnection Connection { get; }
    Task CreateConnectionAsync();
    Task PublishAsync<T>(string exchange, string type, string routingKey, T message);

    Task<(IChannel channel, string QueueName)> CreateReliableQueueAsync(string exchange, string type, string queueName);
    Task CloseAsync();
}

public class RabbitMqConnection : IDisposable, IRabbitMqConnection
{
    private IConnection _connection;
    private readonly ConnectionFactory _factory;
    private readonly ILogger<IRabbitMqConnection> _logger;

    private bool _disposedValue;

    public IConnection Connection => _connection;

    public RabbitMqConnection(ILogger<IRabbitMqConnection> logger) : this(logger, "localhost")
    {
    }

    public RabbitMqConnection(
        string username,
        string password,
        ILogger<IRabbitMqConnection> logger) : this(logger, "localhost", username, password)
    {
    }

    private RabbitMqConnection(
        ILogger<IRabbitMqConnection> logger,
        string hostName,
        string userName = "guest",
        string password = "guest")
    {
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            // Default is 5 sec
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
        };
    }

    public async Task CreateConnectionAsync()
    {
        _connection = await _factory.CreateConnectionAsync();
    }

    public async Task<(IChannel channel, string QueueName)> CreateReliableQueueAsync(string exchange,
        string type,
        string queueName)
    {
        var channel = await _connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(exchange, type);
        
        var queueDeclareResult = await channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);
        
        // Limit amount of requests
        // await channel.ApplyQoS(0, 1, false);

        return (channel, queueDeclareResult.QueueName);
    }

    public async Task PublishAsync<T>(string exchange, string type, string routingKey, T message)
    {
        await using var channel = await _connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(exchange, type);
        await channel.BasicPublishAsync(
            exchange,
            routingKey,
            mandatory: false,
            body: RabbitMqConnectionExtensions.GetMessage(message),
            basicProperties: new BasicProperties
            {
                Persistent = true
            });

        _logger.LogInformation($"Sent '{routingKey} to: {type}@{exchange}");
    }

    public async Task CloseAsync()
    {
        await _connection.CloseAsync();
    }

    #region Disposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            _connection?.Dispose();
        }

        _disposedValue = true;
    }

    #endregion
}

public static class RabbitMqConnectionExtensions
{
    public static byte[] GetMessage<T>(T message)
    {
        return message switch
        {
            string xmlString when IsXml(xmlString) => Encoding.UTF8.GetBytes(xmlString),
            string messageString => Encoding.UTF8.GetBytes(messageString),
            object jsonObject => JsonSerializer.SerializeToUtf8Bytes(message),
            _ => Encoding.UTF8.GetBytes(message?.ToString() ?? string.Empty)
        };
    }

    public static async Task PublishAsync<T>(
        this IRabbitMqConnection connection, string exchange, string routingKey, T message)
        => await connection.PublishAsync(exchange, ExchangeType.Topic, routingKey, message);

    public static async Task PublishAsync<T>(
        this IRabbitMqConnection connection, string exchange, string type)
        => await connection.PublishAsync(exchange, ExchangeType.Fanout, string.Empty, string.Empty);

    public static async Task ApplyQoS(this IChannel channel, uint size, ushort count, bool global)
    {
        await channel.BasicQosAsync(prefetchSize: size, prefetchCount: count, global);
    }
    
    public static async Task<(IChannel channel, string QueueName)> CreateReliableQueueAsync(this IRabbitMqConnection connection, string exchange)
        => await connection.CreateReliableQueueAsync(exchange, ExchangeType.Topic, string.Empty);
    
    public static async Task<(IChannel channel, string QueueName)> CreateReliableQueueAsync(this IRabbitMqConnection connection, string exchange, string type)
        => await connection.CreateReliableQueueAsync(exchange, type, string.Empty);

    #region Private methods

    private static bool IsXml(string text)
    {
        try
        {
            // Try to parse as XML
            _ = XElement.Parse(text);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}