namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// DTO for Auth API response
/// </summary>
public class AuthApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}
