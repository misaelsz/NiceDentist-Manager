using System.ComponentModel.DataAnnotations;

namespace NiceDentist.Manager.Api.DTOs.Requests;

/// <summary>
/// Request DTO for updating an existing appointment
/// </summary>
public class UpdateAppointmentRequest
{
    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be a positive integer")]
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the dentist ID
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "DentistId must be a positive integer")]
    public int DentistId { get; set; }

    /// <summary>
    /// Gets or sets the appointment date and time
    /// </summary>
    [Required]
    public DateTime AppointmentDateTime { get; set; }

    /// <summary>
    /// Gets or sets the type of procedure
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "ProcedureType must be between 2 and 200 characters")]
    public string ProcedureType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional notes for the appointment
    /// </summary>
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string Notes { get; set; } = string.Empty;
}
