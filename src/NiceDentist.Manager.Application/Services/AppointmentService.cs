using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Services;

/// <summary>
/// Service for appointment management operations
/// </summary>
public class AppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDentistRepository _dentistRepository;
    private readonly IEmailService _emailService;

    // Business hours: 8 AM to 6 PM
    private static readonly TimeSpan BusinessStartTime = new(8, 0, 0);
    private static readonly TimeSpan BusinessEndTime = new(18, 0, 0);

    /// <summary>
    /// Initializes a new instance of the AppointmentService
    /// </summary>
    /// <param name="appointmentRepository">Appointment repository</param>
    /// <param name="customerRepository">Customer repository</param>
    /// <param name="dentistRepository">Dentist repository</param>
    /// <param name="emailService">Email service</param>
    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        ICustomerRepository customerRepository,
        IDentistRepository dentistRepository,
        IEmailService emailService)
    {
        _appointmentRepository = appointmentRepository;
        _customerRepository = customerRepository;
        _dentistRepository = dentistRepository;
        _emailService = emailService;
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

        // Check if dentist exists
        var dentist = await _dentistRepository.GetByIdAsync(appointment.DentistId);
        if (dentist == null)
        {
            return (false, "Dentist not found.", null);
        }

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
                dentist.Name,
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
