namespace NiceDentist.Manager.Application.Events;

/// <summary>
/// Event triggered when a customer is created
/// </summary>
public class CustomerCreatedEvent
{
    /// <summary>
    /// Type identifier for the event
    /// </summary>
    public string EventType => "CustomerCreated";
    
    /// <summary>
    /// Unique event ID
    /// </summary>
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Event timestamp
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Customer data
    /// </summary>
    public CustomerCreatedData Data { get; init; } = null!;
}

/// <summary>
/// Data payload for CustomerCreated event
/// </summary>
public class CustomerCreatedData
{
    /// <summary>
    /// Customer ID from Manager database
    /// </summary>
    public int CustomerId { get; init; }
    
    /// <summary>
    /// Customer's full name
    /// </summary>
    public string Name { get; init; } = null!;
    
    /// <summary>
    /// Customer's email address (will be used as username)
    /// </summary>
    public string Email { get; init; } = null!;
    
    /// <summary>
    /// Customer's phone number
    /// </summary>
    public string? Phone { get; init; }
}
