using System.ComponentModel.DataAnnotations;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Api.DTOs.Requests;

/// <summary>
/// Request DTO for updating appointment status
/// </summary>
public class UpdateAppointmentStatusRequest
{
    /// <summary>
    /// Gets or sets the new appointment status
    /// </summary>
    [Required]
    public AppointmentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the reason for status change
    /// </summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
}
