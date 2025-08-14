namespace NiceDentist.Manager.Api.DTOs.Responses;

/// <summary>
/// Response DTO for available time slots
/// </summary>
public class AvailableSlotResponse
{
    /// <summary>
    /// Gets or sets the dentist ID
    /// </summary>
    public int DentistId { get; set; }

    /// <summary>
    /// Gets or sets the dentist name
    /// </summary>
    public string DentistName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the available date and time
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Gets or sets the duration in minutes
    /// </summary>
    public int DurationMinutes { get; set; } = 30;

    /// <summary>
    /// Gets or sets whether this slot is available
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}
