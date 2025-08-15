using System.Text;
using System.Text.Json;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.Events;
using RabbitMQ.Client;

namespace NiceDentist.Manager.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ implementation of event publisher
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly string _exchangeName;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new object();
    
    /// <summary>
    /// Initializes a new instance of RabbitMqEventPublisher
    /// </summary>
    /// <param name="connectionFactory">RabbitMQ connection factory</param>
    /// <param name="exchangeName">Exchange name for publishing events</param>
    public RabbitMqEventPublisher(IConnectionFactory connectionFactory, string exchangeName = "nicedentist.events")
    {
        _connectionFactory = connectionFactory;
        _exchangeName = exchangeName;
    }

    /// <summary>
    /// Ensures connection and channel are available
    /// </summary>
    private void EnsureConnection()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            lock (_lock)
            {
                if (_connection == null || !_connection.IsOpen)
                {
                    try
                    {
                        _connection?.Dispose();
                        _channel?.Dispose();
                        
                        _connection = _connectionFactory.CreateConnection();
                        _channel = _connection.CreateModel();
                        
                        // Declare the exchange (create if it doesn't exist)
                        _channel.ExchangeDeclare(
                            exchange: _exchangeName,
                            type: ExchangeType.Topic,
                            durable: true,
                            autoDelete: false,
                            arguments: null);
                    }
                    catch (Exception)
                    {
                        // If connection fails, set to null so we can retry later
                        _connection = null;
                        _channel = null;
                        throw;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Publishes an event to RabbitMQ
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="eventObject">Event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if published successfully</returns>
    public async Task<bool> PublishAsync<T>(T eventObject, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            EnsureConnection();
            
            if (_channel == null)
            {
                return false;
            }
            
            var json = JsonSerializer.Serialize(eventObject, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            var body = Encoding.UTF8.GetBytes(json);
            
            // Determine routing key based on event type
            var routingKey = GetRoutingKey(eventObject);
            
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            
            // Set the event type for the consumer to identify
            if (eventObject is CustomerCreatedEvent customerEvent)
            {
                properties.Type = customerEvent.EventType;
                Console.WriteLine($"Publishing CustomerCreatedEvent with type: {customerEvent.EventType}");
            }
            else if (eventObject is DentistCreatedEvent dentistEvent)
            {
                properties.Type = dentistEvent.EventType;
                Console.WriteLine($"Publishing DentistCreatedEvent with type: {dentistEvent.EventType}");
            }
            else
            {
                // Fallback to extract event type from the object type name
                properties.Type = eventObject.GetType().Name;
                Console.WriteLine($"Publishing unknown event with type: {eventObject.GetType().Name}");
            }
            
            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);
                
            return await Task.FromResult(true);
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets the routing key for an event
    /// </summary>
    /// <param name="eventObject">Event object</param>
    /// <returns>Routing key</returns>
    private static string GetRoutingKey(object eventObject)
    {
        return eventObject.GetType().Name switch
        {
            "CustomerCreatedEvent" => "customer.created",
            "DentistCreatedEvent" => "dentist.created",
            _ => "unknown.event"
        };
    }
    
    /// <summary>
    /// Disposes resources
    /// </summary>
    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
