namespace NiceDentist.Manager.Application.DTOs;

/// <summary>
/// Data Transfer Object for Customer operations
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the customer
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the customer's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the customer's date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the customer's address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets when the customer was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets when the customer was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the customer is active
    /// </summary>
    public bool IsActive { get; set; }
}
