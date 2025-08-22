using System.Text;
using Audit.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SVC.SharedKernel;

namespace SVC.AuditProcessor;

public class AuditManagerEventConsumer(
    ILogger<AuditManagerEventConsumer> logger,
    IRabbitMqConnection rabbitMqConnection,
    IAuditScopeFactory auditScopeFactory)
    : BackgroundService
{
    private const string ExchangeName = "svc_message_bus";
    private const string RoutingKey = "routing.event.audit.#";
    private string[] InBoundQueuesName { get; } = new string[] { "audit_queue", "app_queue" };

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => { _ = ConsumeAsync(stoppingToken); }, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Stopping AuditProcessorEventConsumer...");

        await rabbitMqConnection.CloseAsync();

        logger.LogInformation("AuditProcessorEventConsumer stopped.");
        await base.StopAsync(stoppingToken);
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation($"Starting ${nameof(AuditManagerEventConsumer)}...");

            await rabbitMqConnection.CreateConnectionAsync();
            var channel = await rabbitMqConnection
                .Connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                cancellationToken: stoppingToken);

            foreach (var queueName in InBoundQueuesName)
            {
                await channel.QueueDeclareAsync(
                    queueName,
                    durable: true,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: ExchangeName,
                    routingKey: RoutingKey,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    logger.LogInformation($"Processing message at: {DateTime.UtcNow} with message id: {ea.BasicProperties.MessageId}");
                    
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        logger.LogInformation($" [Consumer] Received '{ea.RoutingKey}':'{message}'");

                        await using var scope = await auditScopeFactory
                            .CreateAsync(new AuditScopeOptions(config =>
                            {
                                config.EventType(ea.RoutingKey);
                                config.Target(() => new { Data = message });
                            }), stoppingToken);
                        
                        scope.Comment("Audit rabbitmq message");
                        await scope.SaveAsync(stoppingToken);
                        
                        logger.LogInformation("Audit log saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing message");
                    }

                    await Task.FromResult(1);
                };

                await channel.BasicConsumeAsync(
                    queueName,
                    autoAck: true,
                    consumer: consumer,
                    cancellationToken: stoppingToken);
            }


            logger.LogInformation("Consumer started successfully.");

            while (!stoppingToken.IsCancellationRequested)
            {
                // logger.LogInformation("Waiting for messages...");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Critical error in consumer. Restarting...");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            await ExecuteAsync(stoppingToken);
        }
    }
}