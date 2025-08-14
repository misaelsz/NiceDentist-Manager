using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Interface for appointment service operations
/// </summary>
public interface IAppointmentService
{
    /// <summary>
    /// Gets all appointments with optional filtering
    /// </summary>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="customerId">Optional customer ID filter</param>
    /// <param name="dentistId">Optional dentist ID filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>List of appointments</returns>
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync(
        int page = 1, 
        int pageSize = 10, 
        int? customerId = null, 
        int? dentistId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        AppointmentStatus? status = null);

    /// <summary>
    /// Gets an appointment by ID
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Appointment if found, null otherwise</returns>
    Task<Appointment?> GetAppointmentByIdAsync(int id);

    /// <summary>
    /// Gets appointments for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of customer appointments</returns>
    Task<IEnumerable<Appointment>> GetAppointmentsByCustomerAsync(int customerId);

    /// <summary>
    /// Gets appointments for a specific dentist
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <returns>List of dentist appointments</returns>
    Task<IEnumerable<Appointment>> GetAppointmentsByDentistAsync(int dentistId);

    /// <summary>
    /// Creates a new appointment
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="dentistId">Dentist ID</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="procedureType">Type of procedure</param>
    /// <param name="notes">Additional notes</param>
    /// <returns>Created appointment</returns>
    Task<Appointment> CreateAppointmentAsync(int customerId, int dentistId, DateTime appointmentDateTime, string procedureType, string notes = "");

    /// <summary>
    /// Updates an existing appointment
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="customerId">Customer ID</param>
    /// <param name="dentistId">Dentist ID</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="procedureType">Type of procedure</param>
    /// <param name="notes">Additional notes</param>
    /// <returns>Updated appointment if found, null otherwise</returns>
    Task<Appointment?> UpdateAppointmentAsync(int id, int customerId, int dentistId, DateTime appointmentDateTime, string procedureType, string notes = "");

    /// <summary>
    /// Updates the status of an appointment
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="status">New status</param>
    /// <param name="reason">Reason for status change</param>
    /// <returns>Updated appointment if found, null otherwise</returns>
    Task<Appointment?> UpdateAppointmentStatusAsync(int id, AppointmentStatus status, string reason = "");

    /// <summary>
    /// Deletes an appointment
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAppointmentAsync(int id);

    /// <summary>
    /// Gets available time slots for a specific dentist
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <param name="startDate">Start date for availability check</param>
    /// <param name="endDate">End date for availability check</param>
    /// <returns>List of available time slots</returns>
    Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int dentistId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets available time slots for all dentists
    /// </summary>
    /// <param name="startDate">Start date for availability check</param>
    /// <param name="endDate">End date for availability check</param>
    /// <returns>Dictionary of dentist ID and their available slots</returns>
    Task<Dictionary<int, IEnumerable<DateTime>>> GetAllAvailableSlotsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Checks if a time slot is available for a specific dentist
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <param name="dateTime">Date and time to check</param>
    /// <returns>True if available, false otherwise</returns>
    Task<bool> IsSlotAvailableAsync(int dentistId, DateTime dateTime);
}
