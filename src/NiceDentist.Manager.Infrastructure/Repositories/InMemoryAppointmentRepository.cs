using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of appointment repository for testing and development
/// </summary>
public class InMemoryAppointmentRepository : IAppointmentRepository
{
    private readonly List<Appointment> _appointments = new();
    private int _nextId = 1;

    /// <summary>
    /// Creates a new appointment
    /// </summary>
    /// <param name="appointment">The appointment to create</param>
    /// <returns>The created appointment with assigned ID</returns>
    public Task<Appointment> CreateAsync(Appointment appointment)
    {
        appointment.Id = _nextId++;
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;
        _appointments.Add(appointment);
        return Task.FromResult(appointment);
    }

    /// <summary>
    /// Gets an appointment by ID
    /// </summary>
    /// <param name="id">The appointment ID</param>
    /// <returns>The appointment if found, null otherwise</returns>
    public Task<Appointment?> GetByIdAsync(int id)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(appointment);
    }

    /// <summary>
    /// Gets all appointments for a specific customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>List of appointments for the customer</returns>
    public Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId)
    {
        var appointments = _appointments.Where(a => a.CustomerId == customerId).ToList();
        return Task.FromResult<IEnumerable<Appointment>>(appointments);
    }

    /// <summary>
    /// Gets all appointments for a specific dentist
    /// </summary>
    /// <param name="dentistId">The dentist ID</param>
    /// <returns>List of appointments for the dentist</returns>
    public Task<IEnumerable<Appointment>> GetByDentistIdAsync(int dentistId)
    {
        var appointments = _appointments.Where(a => a.DentistId == dentistId).ToList();
        return Task.FromResult<IEnumerable<Appointment>>(appointments);
    }

    /// <summary>
    /// Gets appointments by date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of appointments in the date range</returns>
    public Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var appointments = _appointments.Where(a => 
            a.AppointmentDateTime >= startDate && 
            a.AppointmentDateTime <= endDate).ToList();
        return Task.FromResult<IEnumerable<Appointment>>(appointments);
    }

    /// <summary>
    /// Checks if a customer has conflicting appointments
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="excludeAppointmentId">Appointment ID to exclude from check (for updates)</param>
    /// <returns>True if there's a conflict, false otherwise</returns>
    public Task<bool> HasCustomerConflictAsync(int customerId, DateTime appointmentDateTime, int? excludeAppointmentId = null)
    {
        var conflict = _appointments.Any(a => 
            a.CustomerId == customerId &&
            a.AppointmentDateTime == appointmentDateTime &&
            a.Status != AppointmentStatus.Cancelled &&
            (excludeAppointmentId == null || a.Id != excludeAppointmentId));
        
        return Task.FromResult(conflict);
    }

    /// <summary>
    /// Checks if a dentist has conflicting appointments
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="excludeAppointmentId">Appointment ID to exclude from check (for updates)</param>
    /// <returns>True if there's a conflict, false otherwise</returns>
    public Task<bool> HasDentistConflictAsync(int dentistId, DateTime appointmentDateTime, int? excludeAppointmentId = null)
    {
        var conflict = _appointments.Any(a => 
            a.DentistId == dentistId &&
            a.AppointmentDateTime == appointmentDateTime &&
            a.Status != AppointmentStatus.Cancelled &&
            (excludeAppointmentId == null || a.Id != excludeAppointmentId));
        
        return Task.FromResult(conflict);
    }

    /// <summary>
    /// Updates an appointment
    /// </summary>
    /// <param name="appointment">The appointment to update</param>
    /// <returns>The updated appointment</returns>
    public Task<Appointment> UpdateAsync(Appointment appointment)
    {
        var existingAppointment = _appointments.FirstOrDefault(a => a.Id == appointment.Id);
        if (existingAppointment != null)
        {
            existingAppointment.CustomerId = appointment.CustomerId;
            existingAppointment.DentistId = appointment.DentistId;
            existingAppointment.AppointmentDateTime = appointment.AppointmentDateTime;
            existingAppointment.ProcedureType = appointment.ProcedureType;
            existingAppointment.Notes = appointment.Notes;
            existingAppointment.Status = appointment.Status;
            existingAppointment.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(existingAppointment);
        }
        
        throw new ArgumentException($"Appointment with ID {appointment.Id} not found");
    }

    /// <summary>
    /// Deletes an appointment
    /// </summary>
    /// <param name="id">The appointment ID to delete</param>
    /// <returns>True if deleted, false otherwise</returns>
    public Task<bool> DeleteAsync(int id)
    {
        var appointment = _appointments.FirstOrDefault(a => a.Id == id);
        if (appointment != null)
        {
            _appointments.Remove(appointment);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    /// <summary>
    /// Gets all appointments with filtering and pagination (additional method for service layer)
    /// </summary>
    public IEnumerable<Appointment> GetAll(
        int page = 1, 
        int pageSize = 10, 
        int? customerId = null, 
        int? dentistId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        AppointmentStatus? status = null)
    {
        var query = _appointments.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(a => a.CustomerId == customerId.Value);

        if (dentistId.HasValue)
            query = query.Where(a => a.DentistId == dentistId.Value);

        if (startDate.HasValue)
            query = query.Where(a => a.AppointmentDateTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.AppointmentDateTime <= endDate.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return query
            .OrderBy(a => a.AppointmentDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}
