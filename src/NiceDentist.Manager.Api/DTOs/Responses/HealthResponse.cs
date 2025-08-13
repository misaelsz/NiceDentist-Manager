namespace NiceDentist.Manager.Api.DTOs.Responses;

/// <summary>
/// Response for health check endpoint
/// </summary>
public class HealthResponse
{
    /// <summary>
    /// Gets or sets the overall health status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the health check
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the application version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the uptime of the application
    /// </summary>
    public TimeSpan Uptime { get; set; }

    /// <summary>
    /// Gets or sets detailed service statuses
    /// </summary>
    public Dictionary<string, string> Services { get; set; } = new();
}
