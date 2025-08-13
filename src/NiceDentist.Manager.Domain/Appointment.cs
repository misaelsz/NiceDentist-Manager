namespace NiceDentist.Manager.Domain;

/// <summary>
/// Represents the status of an appointment
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Appointment is scheduled and confirmed
    /// </summary>
    Scheduled = 1,

    /// <summary>
    /// Appointment has been completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Appointment was cancelled
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Appointment cancellation is pending approval
    /// </summary>
    CancellationRequested = 4
}

/// <summary>
/// Represents an appointment in the NiceDentist system
/// </summary>
public class Appointment
{
    /// <summary>
    /// Gets or sets the unique identifier for the appointment
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the dentist ID
    /// </summary>
    public int DentistId { get; set; }

    /// <summary>
    /// Gets or sets the appointment date and time
    /// </summary>
    public DateTime AppointmentDateTime { get; set; }

    /// <summary>
    /// Gets or sets the type of procedure
    /// </summary>
    public string ProcedureType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional notes for the appointment
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the appointment status
    /// </summary>
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    /// <summary>
    /// Gets or sets when the appointment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets when the appointment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for the customer
    /// </summary>
    public virtual Customer Customer { get; set; } = null!;

    /// <summary>
    /// Navigation property for the dentist
    /// </summary>
    public virtual Dentist Dentist { get; set; } = null!;
}
