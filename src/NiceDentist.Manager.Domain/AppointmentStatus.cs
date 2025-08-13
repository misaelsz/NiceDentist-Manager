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
