using System.ComponentModel.DataAnnotations;

namespace NiceDentist.Manager.Api.DTOs.Requests;

/// <summary>
/// Request to complete an appointment
/// </summary>
public class CompleteAppointmentRequest
{
    /// <summary>
    /// Optional completion notes
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
