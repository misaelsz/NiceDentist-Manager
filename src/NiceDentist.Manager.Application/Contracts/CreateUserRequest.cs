namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Model for creating a user in the Auth API
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Gets or sets the first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the auto-generated password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user role
    /// </summary>
    public string Role { get; set; } = string.Empty;
}
