using NiceDentist.Manager.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// Implementation of email service
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailOptions _options;

    /// <summary>
    /// Initializes a new instance of the EmailService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">Email configuration options</param>
    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<bool> SendWelcomeEmailAsync(string email, string name, string username, string password, string role)
    {
        try
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Email service is disabled. Skipping welcome email for: {Email}", email);
                return true; // Consider as success when disabled
            }

            _logger.LogInformation("Sending welcome email to: {Email} for role: {Role}", email, role);

            var subject = "Welcome to NiceDentist - Your Account Details";
            var body = GenerateWelcomeEmailBody(name, username, password, role);

            // For now, just log the email content (in production, use actual SMTP)
            // TODO: Implement actual SMTP sending using System.Net.Mail or MailKit
            _logger.LogInformation("Welcome email content for {Email}:\nSubject: {Subject}\nBody: {Body}", 
                email, subject, body);

            // Simulate async operation
            await Task.Delay(100);

            _logger.LogInformation("Welcome email sent successfully to: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to: {Email}", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendAppointmentConfirmationAsync(string email, string customerName, string dentistName, 
        DateTime appointmentDateTime, string procedureType)
    {
        try
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Email service is disabled. Skipping appointment confirmation email for: {Email}", email);
                return true;
            }

            _logger.LogInformation("Sending appointment confirmation email to: {Email}", email);

            var subject = "Appointment Confirmation - NiceDentist";
            var body = GenerateAppointmentConfirmationEmailBody(customerName, dentistName, appointmentDateTime, procedureType);

            // For now, just log the email content
            _logger.LogInformation("Appointment confirmation email content for {Email}:\nSubject: {Subject}\nBody: {Body}", 
                email, subject, body);

            await Task.Delay(100);

            _logger.LogInformation("Appointment confirmation email sent successfully to: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment confirmation email to: {Email}", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> SendAppointmentCancellationAsync(string email, string customerName, 
        DateTime appointmentDateTime, string procedureType)
    {
        try
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation("Email service is disabled. Skipping appointment cancellation email for: {Email}", email);
                return true;
            }

            _logger.LogInformation("Sending appointment cancellation email to: {Email}", email);

            var subject = "Appointment Cancelled - NiceDentist";
            var body = GenerateAppointmentCancellationEmailBody(customerName, appointmentDateTime, procedureType);

            // For now, just log the email content
            _logger.LogInformation("Appointment cancellation email content for {Email}:\nSubject: {Subject}\nBody: {Body}", 
                email, subject, body);

            await Task.Delay(100);

            _logger.LogInformation("Appointment cancellation email sent successfully to: {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send appointment cancellation email to: {Email}", email);
            return false;
        }
    }

    /// <summary>
    /// Generates welcome email body
    /// </summary>
    /// <param name="name">User name</param>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <param name="role">User role</param>
    /// <returns>Email body</returns>
    private static string GenerateWelcomeEmailBody(string name, string username, string password, string role)
    {
        return $@"
Dear {name},

Welcome to NiceDentist! Your account has been created successfully.

Your login details:
- Username: {username}
- Password: {password}
- Role: {role}

Please keep these credentials safe and change your password after your first login.

You can access the system at: [System URL]

If you have any questions, please contact our support team.

Best regards,
NiceDentist Team
";
    }

    /// <summary>
    /// Generates appointment confirmation email body
    /// </summary>
    /// <param name="customerName">Customer name</param>
    /// <param name="dentistName">Dentist name</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="procedureType">Procedure type</param>
    /// <returns>Email body</returns>
    private static string GenerateAppointmentConfirmationEmailBody(string customerName, string dentistName, 
        DateTime appointmentDateTime, string procedureType)
    {
        return $@"
Dear {customerName},

Your appointment has been confirmed!

Appointment Details:
- Date & Time: {appointmentDateTime:dddd, MMMM dd, yyyy 'at' HH:mm}
- Dentist: Dr. {dentistName}
- Procedure: {procedureType}

Please arrive 15 minutes before your scheduled appointment time.

If you need to reschedule or cancel, please contact us as soon as possible.

Best regards,
NiceDentist Team
";
    }

    /// <summary>
    /// Generates appointment cancellation email body
    /// </summary>
    /// <param name="customerName">Customer name</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="procedureType">Procedure type</param>
    /// <returns>Email body</returns>
    private static string GenerateAppointmentCancellationEmailBody(string customerName, 
        DateTime appointmentDateTime, string procedureType)
    {
        return $@"
Dear {customerName},

Your appointment has been cancelled.

Cancelled Appointment Details:
- Date & Time: {appointmentDateTime:dddd, MMMM dd, yyyy 'at' HH:mm}
- Procedure: {procedureType}

If you would like to reschedule, please contact us at your convenience.

Best regards,
NiceDentist Team
";
    }
}
