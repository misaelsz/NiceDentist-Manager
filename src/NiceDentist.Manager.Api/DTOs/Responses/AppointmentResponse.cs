using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Api.DTOs.Responses;

/// <summary>
/// Response DTO for appointment data
/// </summary>
public class AppointmentResponse
{
    /// <summary>
    /// Gets or sets the appointment ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the customer ID
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dentist ID
    /// </summary>
    public int DentistId { get; set; }

    /// <summary>
    /// Gets or sets the dentist name
    /// </summary>
    public string DentistName { get; set; } = string.Empty;

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
    /// Gets or sets the appointment status as text
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the appointment was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the appointment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
