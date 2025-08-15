using System.ComponentModel.DataAnnotations;

namespace NiceDentist.Manager.Api.DTOs.Requests;

/// <summary>
/// Request to cancel an appointment
/// </summary>
public class CancelAppointmentRequest
{
    /// <summary>
    /// Optional reason for cancellation
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }
}
