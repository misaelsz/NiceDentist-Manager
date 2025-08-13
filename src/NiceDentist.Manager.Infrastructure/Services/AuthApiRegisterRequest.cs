namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// DTO for Auth API user registration request
/// </summary>
public class AuthApiRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
