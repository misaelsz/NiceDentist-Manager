namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Service interface for email operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a welcome email with auto-generated password to a new user
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="name">Recipient name</param>
    /// <param name="username">Username for login</param>
    /// <param name="password">Auto-generated password</param>
    /// <param name="role">User role</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    Task<bool> SendWelcomeEmailAsync(string email, string name, string username, string password, string role);

    /// <summary>
    /// Sends an appointment confirmation email
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="customerName">Customer name</param>
    /// <param name="dentistName">Dentist name</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="procedureType">Type of procedure</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    Task<bool> SendAppointmentConfirmationAsync(string email, string customerName, string dentistName, 
        DateTime appointmentDateTime, string procedureType);

    /// <summary>
    /// Sends an appointment cancellation notification email
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="customerName">Customer name</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="procedureType">Type of procedure</param>
    /// <returns>True if email was sent successfully, false otherwise</returns>
    Task<bool> SendAppointmentCancellationAsync(string email, string customerName, 
        DateTime appointmentDateTime, string procedureType);
}
