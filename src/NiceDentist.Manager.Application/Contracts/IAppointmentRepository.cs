using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Repository interface for Appointment operations
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Creates a new appointment
    /// </summary>
    /// <param name="appointment">The appointment to create</param>
    /// <returns>The created appointment with assigned ID</returns>
    Task<Appointment> CreateAsync(Appointment appointment);

    /// <summary>
    /// Gets an appointment by ID
    /// </summary>
    /// <param name="id">The appointment ID</param>
    /// <returns>The appointment if found, null otherwise</returns>
    Task<Appointment?> GetByIdAsync(int id);

    /// <summary>
    /// Gets all appointments for a specific customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>List of appointments for the customer</returns>
    Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId);

    /// <summary>
    /// Gets all appointments for a specific dentist
    /// </summary>
    /// <param name="dentistId">The dentist ID</param>
    /// <returns>List of appointments for the dentist</returns>
    Task<IEnumerable<Appointment>> GetByDentistIdAsync(int dentistId);

    /// <summary>
    /// Gets appointments by date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of appointments in the date range</returns>
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Checks if a customer has conflicting appointments
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="excludeAppointmentId">Appointment ID to exclude from check (for updates)</param>
    /// <returns>True if there's a conflict, false otherwise</returns>
    Task<bool> HasCustomerConflictAsync(int customerId, DateTime appointmentDateTime, int? excludeAppointmentId = null);

    /// <summary>
    /// Checks if a dentist has conflicting appointments
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="excludeAppointmentId">Appointment ID to exclude from check (for updates)</param>
    /// <returns>True if there's a conflict, false otherwise</returns>
    Task<bool> HasDentistConflictAsync(int dentistId, DateTime appointmentDateTime, int? excludeAppointmentId = null);

    /// <summary>
    /// Updates an appointment
    /// </summary>
    /// <param name="appointment">The appointment to update</param>
    /// <returns>The updated appointment</returns>
    Task<Appointment> UpdateAsync(Appointment appointment);

    /// <summary>
    /// Deletes an appointment
    /// </summary>
    /// <param name="id">The appointment ID to delete</param>
    /// <returns>True if deleted, false otherwise</returns>
    Task<bool> DeleteAsync(int id);
}
