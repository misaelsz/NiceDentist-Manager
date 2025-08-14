namespace NiceDentist.Manager.Application.Events;

/// <summary>
/// Event received when a user is created in the Auth API
/// This event contains the UserId that should be linked with Customer/Dentist
/// </summary>
public class UserCreatedEvent
{
    /// <summary>
    /// Type identifier for the event
    /// </summary>
    public static string EventType => "UserCreated";
    
    /// <summary>
    /// Unique event ID
    /// </summary>
    public string EventId { get; init; } = string.Empty;
    
    /// <summary>
    /// Event timestamp
    /// </summary>
    public DateTime Timestamp { get; init; }
    
    /// <summary>
    /// User data from Auth API
    /// </summary>
    public UserCreatedData Data { get; init; } = null!;
}

/// <summary>
/// Data payload for UserCreated event
/// </summary>
public class UserCreatedData
{
    /// <summary>
    /// User ID from Auth database
    /// </summary>
    public int UserId { get; init; }
    
    /// <summary>
    /// User's email address (used to match with Customer/Dentist)
    /// </summary>
    public string Email { get; init; } = null!;
    
    /// <summary>
    /// User's role (Customer or Dentist)
    /// </summary>
    public string Role { get; init; } = null!;
    
    /// <summary>
    /// Type of entity in Manager API (Customer or Dentist)
    /// </summary>
    public string EntityType { get; init; } = null!;
    
    /// <summary>
    /// ID of the entity in Manager API that should be linked
    /// </summary>
    public int EntityId { get; init; }
}
