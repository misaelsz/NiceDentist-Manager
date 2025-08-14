using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Services;

/// <summary>
/// Service for appointment management operations
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IEmailService _emailService;

    // Business hours: 8 AM to 6 PM
    private static readonly TimeSpan BusinessStartTime = new(8, 0, 0);
    private static readonly TimeSpan BusinessEndTime = new(18, 0, 0);
    private const string AppointmentNotFoundMessage = "Appointment not found.";

    /// <summary>
    /// Initializes a new instance of the AppointmentService
    /// </summary>
    /// <param name="appointmentRepository">Appointment repository</param>
    /// <param name="customerRepository">Customer repository</param>
    /// <param name="emailService">Email service</param>
    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        ICustomerRepository customerRepository,
        IEmailService emailService)
    {
        _appointmentRepository = appointmentRepository;
        _customerRepository = customerRepository;
        _emailService = emailService;
    }

    /// <summary>
    /// Gets all appointments with optional filtering
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync(
        int page = 1, 
        int pageSize = 10, 
        int? customerId = null, 
        int? dentistId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        AppointmentStatus? status = null)
    {
        // Try to use GetAllAsync method if repository supports it
        var repositoryType = _appointmentRepository.GetType();
        var getAllMethod = repositoryType.GetMethod("GetAllAsync");
        
        if (getAllMethod != null)
        {
            var result = getAllMethod.Invoke(_appointmentRepository, new object[] { page, pageSize, customerId, dentistId, startDate, endDate, status });
            if (result is Task<IEnumerable<Appointment>> taskResult)
            {
                return await taskResult;
            }
        }

        // Fallback implementation for other repository types
        var appointmentsList = new List<Appointment>();
        
        if (customerId.HasValue)
        {
            appointmentsList.AddRange(await _appointmentRepository.GetByCustomerIdAsync(customerId.Value));
        }
        else if (dentistId.HasValue)
        {
            appointmentsList.AddRange(await _appointmentRepository.GetByDentistIdAsync(dentistId.Value));
        }
        else if (startDate.HasValue && endDate.HasValue)
        {
            appointmentsList.AddRange(await _appointmentRepository.GetByDateRangeAsync(startDate.Value, endDate.Value));
        }

        return appointmentsList.Where(a => 
                (!status.HasValue || a.Status == status.Value) &&
                (!startDate.HasValue || a.AppointmentDateTime >= startDate.Value) &&
                (!endDate.HasValue || a.AppointmentDateTime <= endDate.Value))
            .OrderBy(a => a.AppointmentDateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Gets appointments for a specific customer
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetAppointmentsByCustomerAsync(int customerId)
    {
        return await _appointmentRepository.GetByCustomerIdAsync(customerId);
    }

    /// <summary>
    /// Gets appointments for a specific dentist
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetAppointmentsByDentistAsync(int dentistId)
    {
        return await _appointmentRepository.GetByDentistIdAsync(dentistId);
    }

    /// <summary>
    /// Creates a new appointment
    /// </summary>
    public async Task<Appointment> CreateAppointmentAsync(int customerId, int dentistId, DateTime appointmentDateTime, string procedureType, string notes = "")
    {
        var appointment = new Appointment
        {
            CustomerId = customerId,
            DentistId = dentistId,
            AppointmentDateTime = appointmentDateTime,
            ProcedureType = procedureType,
            Notes = notes,
            Status = AppointmentStatus.Scheduled
        };

        var result = await CreateAppointmentAsync(appointment);
        if (!result.Success)
        {
            throw new ArgumentException(result.Message);
        }

        return result.Appointment!;
    }

    /// <summary>
    /// Updates an existing appointment
    /// </summary>
    public async Task<Appointment?> UpdateAppointmentAsync(int id, int customerId, int dentistId, DateTime appointmentDateTime, string procedureType, string notes = "")
    {
        var existingAppointment = await _appointmentRepository.GetByIdAsync(id);
        if (existingAppointment == null)
        {
            return null;
        }

        existingAppointment.CustomerId = customerId;
        existingAppointment.DentistId = dentistId;
        existingAppointment.AppointmentDateTime = appointmentDateTime;
        existingAppointment.ProcedureType = procedureType;
        existingAppointment.Notes = notes;

        var result = await UpdateAppointmentAsync(existingAppointment);
        if (!result.Success)
        {
            throw new ArgumentException(result.Message);
        }

        return existingAppointment;
    }

    /// <summary>
    /// Updates the status of an appointment
    /// </summary>
    public async Task<Appointment?> UpdateAppointmentStatusAsync(int id, AppointmentStatus status, string reason = "")
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            return null;
        }

        appointment.Status = status;
        appointment.UpdatedAt = DateTime.UtcNow;
        
        await _appointmentRepository.UpdateAsync(appointment);
        return appointment;
    }

    /// <summary>
    /// Deletes an appointment
    /// </summary>
    public async Task<bool> DeleteAppointmentAsync(int id)
    {
        return await _appointmentRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Gets available time slots for a specific dentist
    /// </summary>
    public async Task<IEnumerable<DateTime>> GetAvailableSlotsAsync(int dentistId, DateTime startDate, DateTime endDate)
    {
        var availableSlots = new List<DateTime>();
        var current = startDate.Date.Add(BusinessStartTime);
        var endDateTime = endDate.Date.Add(BusinessEndTime);

        while (current <= endDateTime)
        {
            // Skip weekends
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                // Check if time is within business hours
                if (current.TimeOfDay >= BusinessStartTime && current.TimeOfDay < BusinessEndTime)
                {
                    // Check if slot is not occupied
                    var hasConflict = await _appointmentRepository.HasDentistConflictAsync(dentistId, current);
                    if (!hasConflict)
                    {
                        availableSlots.Add(current);
                    }
                }
            }

            current = current.AddMinutes(30); // 30-minute slots
            
            // Move to next day at business start time if we've passed business end time
            if (current.TimeOfDay >= BusinessEndTime)
            {
                current = current.Date.AddDays(1).Add(BusinessStartTime);
            }
        }

        return availableSlots;
    }

    /// <summary>
    /// Gets available time slots for all dentists
    /// </summary>
    public async Task<Dictionary<int, IEnumerable<DateTime>>> GetAllAvailableSlotsAsync(DateTime startDate, DateTime endDate)
    {
        // For now, we'll assume dentists 1, 2, 3 exist
        // In a real implementation, this would query the dentist repository
        var dentistIds = new[] { 1, 2, 3 };
        var result = new Dictionary<int, IEnumerable<DateTime>>();

        foreach (var dentistId in dentistIds)
        {
            var slots = await GetAvailableSlotsAsync(dentistId, startDate, endDate);
            result[dentistId] = slots;
        }

        return result;
    }

    /// <summary>
    /// Checks if a time slot is available for a specific dentist
    /// </summary>
    public async Task<bool> IsSlotAvailableAsync(int dentistId, DateTime dateTime)
    {
        // Check business hours
        var validation = ValidateAppointmentDateTime(dateTime);
        if (!validation.IsValid)
        {
            return false;
        }

        // Check for conflicts
        return !await _appointmentRepository.HasDentistConflictAsync(dentistId, dateTime);
    }

    /// <summary>
    /// Creates a new appointment
    /// </summary>
    /// <param name="appointment">Appointment to create</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message, Appointment? Appointment)> CreateAppointmentAsync(Appointment appointment)
    {
        // Validate basic data
        if (appointment.CustomerId <= 0 || appointment.DentistId <= 0)
        {
            return (false, "Invalid customer or dentist ID.", null);
        }

        if (string.IsNullOrWhiteSpace(appointment.ProcedureType))
        {
            return (false, "Procedure type is required.", null);
        }

        // Validate date and time
        var validationResult = ValidateAppointmentDateTime(appointment.AppointmentDateTime);
        if (!validationResult.IsValid)
        {
            return (false, validationResult.Message, null);
        }

        // Check if customer exists
        var customer = await _customerRepository.GetByIdAsync(appointment.CustomerId);
        if (customer == null)
        {
            return (false, "Customer not found.", null);
        }

        // Check if dentist exists (for now, assume dentist exists)
        // TODO: Add dentist repository when dentist implementation is ready

        // Check for conflicts
        var hasCustomerConflict = await _appointmentRepository.HasCustomerConflictAsync(
            appointment.CustomerId, appointment.AppointmentDateTime);
        if (hasCustomerConflict)
        {
            return (false, "Customer already has an appointment at this time.", null);
        }

        var hasDentistConflict = await _appointmentRepository.HasDentistConflictAsync(
            appointment.DentistId, appointment.AppointmentDateTime);
        if (hasDentistConflict)
        {
            return (false, "Dentist already has an appointment at this time.", null);
        }

        try
        {
            var createdAppointment = await _appointmentRepository.CreateAsync(appointment);

            // Send confirmation email to customer
            await _emailService.SendAppointmentConfirmationAsync(
                customer.Email,
                customer.Name,
                "Dentist", // TODO: Get actual dentist name when dentist implementation is ready
                appointment.AppointmentDateTime,
                appointment.ProcedureType);

            return (true, "Appointment created successfully.", createdAppointment);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to create appointment: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Gets appointments for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of customer appointments</returns>
    public async Task<IEnumerable<Appointment>> GetCustomerAppointmentsAsync(int customerId)
    {
        return await _appointmentRepository.GetByCustomerIdAsync(customerId);
    }

    /// <summary>
    /// Gets appointments for a specific dentist
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <returns>List of dentist appointments</returns>
    public async Task<IEnumerable<Appointment>> GetDentistAppointmentsAsync(int dentistId)
    {
        return await _appointmentRepository.GetByDentistIdAsync(dentistId);
    }

    /// <summary>
    /// Gets an appointment by ID
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Appointment if found, null otherwise</returns>
    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _appointmentRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Requests cancellation of an appointment (by customer)
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <param name="customerId">Customer ID requesting cancellation</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> RequestAppointmentCancellationAsync(int appointmentId, int customerId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            return (false, "Appointment not found.");
        }

        if (appointment.CustomerId != customerId)
        {
            return (false, "You can only cancel your own appointments.");
        }

        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            return (false, "Only scheduled appointments can be cancelled.");
        }

        appointment.Status = AppointmentStatus.CancellationRequested;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);

        return (true, "Cancellation request submitted successfully.");
    }

    /// <summary>
    /// Cancels an appointment (by admin/manager)
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            return (false, "Appointment not found.");
        }

        if (appointment.Status == AppointmentStatus.Completed)
        {
            return (false, "Cannot cancel completed appointments.");
        }

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);

        // Get customer for notification
        var customer = await _customerRepository.GetByIdAsync(appointment.CustomerId);
        if (customer != null)
        {
            await _emailService.SendAppointmentCancellationAsync(
                customer.Email,
                customer.Name,
                appointment.AppointmentDateTime,
                appointment.ProcedureType);
        }

        return (true, "Appointment cancelled successfully.");
    }

    /// <summary>
    /// Completes an appointment
    /// </summary>
    /// <param name="appointmentId">Appointment ID</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> CompleteAppointmentAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            return (false, "Appointment not found.");
        }

        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            return (false, "Only scheduled appointments can be completed.");
        }

        appointment.Status = AppointmentStatus.Completed;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);

        return (true, "Appointment marked as completed.");
    }

    /// <summary>
    /// Updates an appointment
    /// </summary>
    /// <param name="appointment">Appointment to update</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> UpdateAppointmentAsync(Appointment appointment)
    {
        if (appointment.Id <= 0)
        {
            return (false, "Invalid appointment ID.");
        }

        var existingAppointment = await _appointmentRepository.GetByIdAsync(appointment.Id);
        if (existingAppointment == null)
        {
            return (false, "Appointment not found.");
        }

        // Validate date and time if changed
        if (existingAppointment.AppointmentDateTime != appointment.AppointmentDateTime)
        {
            var validationResult = ValidateAppointmentDateTime(appointment.AppointmentDateTime);
            if (!validationResult.IsValid)
            {
                return (false, validationResult.Message);
            }

            // Check for conflicts (excluding current appointment)
            var hasCustomerConflict = await _appointmentRepository.HasCustomerConflictAsync(
                appointment.CustomerId, appointment.AppointmentDateTime, appointment.Id);
            if (hasCustomerConflict)
            {
                return (false, "Customer already has an appointment at this time.");
            }

            var hasDentistConflict = await _appointmentRepository.HasDentistConflictAsync(
                appointment.DentistId, appointment.AppointmentDateTime, appointment.Id);
            if (hasDentistConflict)
            {
                return (false, "Dentist already has an appointment at this time.");
            }
        }

        appointment.UpdatedAt = DateTime.UtcNow;
        await _appointmentRepository.UpdateAsync(appointment);

        return (true, "Appointment updated successfully.");
    }

    /// <summary>
    /// Validates appointment date and time
    /// </summary>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <returns>Validation result</returns>
    private static (bool IsValid, string Message) ValidateAppointmentDateTime(DateTime appointmentDateTime)
    {
        // Cannot schedule in the past
        if (appointmentDateTime <= DateTime.Now)
        {
            return (false, "Cannot schedule appointments in the past.");
        }

        // Check business hours
        var timeOfDay = appointmentDateTime.TimeOfDay;
        if (timeOfDay < BusinessStartTime || timeOfDay >= BusinessEndTime)
        {
            return (false, $"Appointments can only be scheduled between {BusinessStartTime:hh\\:mm} and {BusinessEndTime:hh\\:mm}.");
        }

        // Check if it's weekend (Saturday or Sunday)
        if (appointmentDateTime.DayOfWeek == DayOfWeek.Saturday || appointmentDateTime.DayOfWeek == DayOfWeek.Sunday)
        {
            return (false, "Appointments cannot be scheduled on weekends.");
        }

        return (true, string.Empty);
    }
}
