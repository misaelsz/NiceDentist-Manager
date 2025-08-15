namespace NiceDentist.Manager.Application.Events;

/// <summary>
/// Event triggered when a dentist is created
/// This event is sent to the Auth API to create a user account
/// </summary>
public class DentistCreatedEvent
{
    /// <summary>
    /// Type identifier for the event
    /// </summary>
    public string EventType => "dentistcreated";

    /// <summary>
    /// Unique event ID
    /// </summary>
    public string EventId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Event timestamp
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Dentist data to be sent to Auth API
    /// </summary>
    public DentistCreatedData Data { get; init; } = null!;
}

/// <summary>
/// Data payload for DentistCreated event
/// </summary>
public class DentistCreatedData
{
    /// <summary>
    /// Dentist ID from Manager database
    /// </summary>
    public int DentistId { get; init; }

    /// <summary>
    /// Dentist's full name
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Dentist's email address (will be used as username in Auth)
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Dentist's license number
    /// </summary>
    public string LicenseNumber { get; init; } = null!;

    /// <summary>
    /// Dentist's specialization
    /// </summary>
    public string Specialization { get; init; } = null!;
}
