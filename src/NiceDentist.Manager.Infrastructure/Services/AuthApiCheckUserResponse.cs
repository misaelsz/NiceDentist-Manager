namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// DTO for Auth API check user response
/// </summary>
public class AuthApiCheckUserResponse
{
    public bool Exists { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}
