using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NiceDentist.Manager.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of event consumer
/// Runs as a background service to consume events
/// </summary>
public class RabbitMqEventConsumer : BackgroundService, IEventConsumer
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqEventConsumer> _logger;
    private readonly string _queueName;

    /// <summary>
    /// Initializes a new instance of RabbitMqEventConsumer
    /// </summary>
    /// <param name="connectionFactory">RabbitMQ connection factory</param>
    /// <param name="serviceProvider">Service provider for resolving handlers</param>
    /// <param name="logger">Logger</param>
    /// <param name="exchangeName">Exchange name</param>
    /// <param name="queueName">Queue name for this service</param>
    public RabbitMqEventConsumer(
        IConnectionFactory connectionFactory,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqEventConsumer> logger,
        string exchangeName = "nicedentist.events",
        string queueName = "manager.user.created")
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queueName = queueName;

        try
        {
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            
            // Declare exchange
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic, durable: true);
            
            // Declare queue (Auth API publishes directly to this queue)
            _channel.QueueDeclare(queue: _queueName, durable: true, exclusive: false, autoDelete: false);
            
            // Note: Auth API publishes directly to queue, not via exchange routing
            // So we don't need to bind the queue to the exchange
            
            _logger.LogInformation("RabbitMQ Event Consumer initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Event Consumer");
            throw new InvalidOperationException("Failed to initialize RabbitMQ Event Consumer", ex);
        }
    }

    /// <summary>
    /// Starts the background service to consume events
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartAsync(stoppingToken);
        
        // Keep the service running until cancellation is requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    /// <summary>
    /// Starts consuming events from RabbitMQ
    /// </summary>
    public new Task StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    // When publishing directly to queue, we need to check the message type
                    // from the properties or deserialize to determine event type
                    var eventType = ea.BasicProperties?.Type ?? "Unknown";

                    _logger.LogInformation("Received event of type: {EventType}", eventType);

                    await ProcessEventAsync(eventType, message);

                    // Acknowledge the message
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event");
                    
                    // Reject and requeue the message for retry
                    _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
            
            _logger.LogInformation("Started consuming events from queue: {QueueName}", _queueName);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start event consumer");
            throw new InvalidOperationException("Failed to start RabbitMQ event consumer", ex);
        }
    }

    /// <summary>
    /// Stops consuming events from RabbitMQ
    /// </summary>
    public new Task StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Stopping event consumer");
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping event consumer");
            throw new InvalidOperationException("Failed to stop RabbitMQ event consumer", ex);
        }
    }

    /// <summary>
    /// Processes an event based on its event type
    /// </summary>
    private async Task ProcessEventAsync(string eventType, string message)
    {
        try
        {
            switch (eventType)
            {
                case "UserCreated":
                    await ProcessUserCreatedEventAsync(message);
                    break;
                
                default:
                    _logger.LogWarning("Unknown event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event of type: {EventType}", eventType);
            throw new InvalidOperationException($"Failed to process event of type: {eventType}", ex);
        }
    }

    /// <summary>
    /// Processes UserCreated events
    /// </summary>
    private async Task ProcessUserCreatedEventAsync(string message)
    {
        try
        {
            var eventObject = JsonSerializer.Deserialize<UserCreatedEvent>(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (eventObject == null)
            {
                _logger.LogError("Failed to deserialize UserCreated event");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IEventHandler<UserCreatedEvent>>();
            
            var success = await handler.HandleAsync(eventObject);
            
            if (!success)
            {
                _logger.LogError("Handler returned failure for UserCreated event");
                throw new InvalidOperationException("Event handler returned failure");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserCreated event");
            throw new InvalidOperationException("Failed to process UserCreated event", ex);
        }
    }

    /// <summary>
    /// Disposes resources
    /// </summary>
    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
