namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Model for creating a user in the Auth API
/// </summary>
public class CreateUserRequest
{
    /// <summary>
    /// Gets or sets the username
    /// </summary>
    public string Username { get; set; } = string.Empty;

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

/// <summary>
/// Service interface for Auth API operations
/// </summary>
public interface IAuthApiService
{
    /// <summary>
    /// Creates a new user in the Auth API
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>True if user was created successfully, false otherwise</returns>
    Task<bool> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// Deletes a user from the Auth API by email
    /// </summary>
    /// <param name="email">User email to delete</param>
    /// <returns>True if user was deleted successfully, false otherwise</returns>
    Task<bool> DeleteUserByEmailAsync(string email);

    /// <summary>
    /// Checks if a user exists in the Auth API by email
    /// </summary>
    /// <param name="email">User email to check</param>
    /// <returns>True if user exists, false otherwise</returns>
    Task<bool> UserExistsByEmailAsync(string email);
}
