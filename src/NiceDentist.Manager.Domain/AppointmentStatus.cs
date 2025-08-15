using System.ComponentModel;

namespace NiceDentist.Manager.Domain;

/// <summary>
/// Represents the status of an appointment
/// </summary>
public enum AppointmentStatus
{
    /// <summary>
    /// Appointment is scheduled and confirmed
    /// </summary>
    [Description("Scheduled")]
    Scheduled = 1,

    /// <summary>
    /// Appointment has been completed
    /// </summary>
    [Description("Completed")]
    Completed = 2,

    /// <summary>
    /// Appointment was cancelled
    /// </summary>
    [Description("Cancelled")]
    Cancelled = 3,

    /// <summary>
    /// Appointment cancellation is pending approval
    /// </summary>
    [Description("Cancellation Requested")]
    CancellationRequested = 4
}
