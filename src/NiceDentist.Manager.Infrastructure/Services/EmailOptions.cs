namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// Configuration options for Email service
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets or sets the SMTP server host
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server port
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Gets or sets whether to use SSL
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Gets or sets the sender email address
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender name
    /// </summary>
    public string SenderName { get; set; } = "NiceDentist System";

    /// <summary>
    /// Gets or sets the SMTP username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether email service is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
}
