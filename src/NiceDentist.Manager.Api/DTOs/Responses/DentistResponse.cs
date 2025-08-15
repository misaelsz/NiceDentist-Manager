namespace NiceDentist.Manager.Api.DTOs.Responses;

/// <summary>
/// Response object for Dentist endpoints
/// </summary>
public class DentistResponse
{
    /// <summary>
    /// Gets or sets the dentist ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the dentist's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dentist's email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dentist's phone
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dentist's license number
    /// </summary>
    public string LicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the dentist's specialization
    /// </summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the dentist was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the dentist was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the dentist is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the user ID from Auth API
    /// </summary>
    public int? UserId { get; set; }
}
