using NiceDentist.Manager.Application.Contracts;

namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// Mock implementation of IEmailService for testing and development
/// </summary>
public class MockEmailService : IEmailService
{
    /// <summary>
    /// Sends a welcome email
    /// </summary>
    /// <param name="email">Recipient email</param>
    /// <param name="name">Recipient name</param>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <param name="role">User role</param>
    /// <returns>Always returns true for mock</returns>
    public Task<bool> SendWelcomeEmailAsync(string email, string name, string username, string password, string role)
    {
        // Mock implementation - just log or do nothing
        Console.WriteLine($"Mock: Welcome email sent to {email} for {name} with role {role}");
        return Task.FromResult(true);
    }

    /// <summary>
    /// Sends an appointment confirmation email
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="customerName">Customer name</param>
    /// <param name="dentistName">Dentist name</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="procedureType">Type of procedure</param>
    /// <returns>Always returns true for mock</returns>
    public Task<bool> SendAppointmentConfirmationAsync(string email, string customerName, string dentistName, 
        DateTime appointmentDateTime, string procedureType)
    {
        Console.WriteLine($"Mock: Appointment confirmation email sent to {email} for {customerName}");
        return Task.FromResult(true);
    }

    /// <summary>
    /// Sends an appointment cancellation notification email
    /// </summary>
    /// <param name="email">Recipient email address</param>
    /// <param name="customerName">Customer name</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="procedureType">Type of procedure</param>
    /// <returns>Always returns true for mock</returns>
    public Task<bool> SendAppointmentCancellationAsync(string email, string customerName, 
        DateTime appointmentDateTime, string procedureType)
    {
        Console.WriteLine($"Mock: Appointment cancellation email sent to {email} for {customerName}");
        return Task.FromResult(true);
    }
}
