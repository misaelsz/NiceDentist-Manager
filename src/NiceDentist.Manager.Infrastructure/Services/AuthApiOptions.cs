namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// Configuration options for Auth API
/// </summary>
public class AuthApiOptions
{
    /// <summary>
    /// Gets or sets the base URL of the Auth API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key for authentication
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout for HTTP requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
