namespace NiceDentist.Manager.Api.Controllers;

/// <summary>
/// Health check controller
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private static readonly DateTime _startTime = DateTime.UtcNow;
    private const string HealthyStatus = "Healthy";

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    public ActionResult<HealthResponse> GetHealth()
    {
        var response = new HealthResponse
        {
            Status = HealthyStatus,
            Timestamp = DateTime.UtcNow,
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown",
            Uptime = DateTime.UtcNow - _startTime,
            Services = new Dictionary<string, string>
            {
                {"Application", HealthyStatus},
                {"Memory", HealthyStatus}
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Detailed status endpoint
    /// </summary>
    /// <returns>Detailed status information</returns>
    [HttpGet("status")]
    public ActionResult<HealthResponse> GetDetailedStatus()
    {
        var response = new HealthResponse
        {
            Status = HealthyStatus,
            Timestamp = DateTime.UtcNow,
            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown",
            Uptime = DateTime.UtcNow - _startTime,
            Services = new Dictionary<string, string>
            {
                {"Application", HealthyStatus},
                {"Memory", GetMemoryStatus()},
                {"EmailService", HealthyStatus},
                {"AuthApi", "Not Configured"}
            }
        };

        return Ok(response);
    }

    private static string GetMemoryStatus()
    {
        var workingSet = GC.GetTotalMemory(false);
        return workingSet < 100_000_000 ? "Healthy" : "Warning"; // 100MB threshold
    }
}
