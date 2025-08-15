using Microsoft.AspNetCore.Mvc;
using NiceDentist.Manager.Api.DTOs.Requests;
using NiceDentist.Manager.Api.DTOs.Responses;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;
using NiceDentist.Manager.Domain.Extensions;

namespace NiceDentist.Manager.Api.Controllers;

/// <summary>
/// Controller for managing appointments
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentsController> _logger;
    private const string GenericErrorMessage = "An error occurred while processing your request.";

    public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all appointments with optional filtering
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10)</param>
    /// <param name="customerId">Filter by customer ID</param>
    /// <param name="dentistId">Filter by dentist ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <param name="status">Filter by status</param>
    /// <returns>List of appointments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAllAppointments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? customerId = null,
        [FromQuery] int? dentistId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] AppointmentStatus? status = null)
    {
        try
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync(page, pageSize, customerId, dentistId, startDate, endDate, status);
            var response = appointments.Select(MapToResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting appointments");
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Gets an appointment by ID
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>Appointment details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppointmentResponse>> GetAppointmentById(int id)
    {
        try
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound($"Appointment with ID {id} not found");
            }

            return Ok(MapToResponse(appointment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting appointment {AppointmentId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Gets appointments for a specific customer
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <returns>List of customer appointments</returns>
    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointmentsByCustomer(int customerId)
    {
        try
        {
            var appointments = await _appointmentService.GetAppointmentsByCustomerAsync(customerId);
            var response = appointments.Select(MapToResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting appointments for customer {CustomerId}", customerId);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Gets appointments for a specific dentist
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <returns>List of dentist appointments</returns>
    [HttpGet("dentist/{dentistId}")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AppointmentResponse>>> GetAppointmentsByDentist(int dentistId)
    {
        try
        {
            var appointments = await _appointmentService.GetAppointmentsByDentistAsync(dentistId);
            var response = appointments.Select(MapToResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting appointments for dentist {DentistId}", dentistId);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Gets available time slots for a specific dentist
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <param name="startDate">Start date for availability check</param>
    /// <param name="endDate">End date for availability check</param>
    /// <returns>List of available time slots</returns>
    [HttpGet("dentist/{dentistId}/available-slots")]
    [ProducesResponseType(typeof(IEnumerable<AvailableSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AvailableSlotResponse>>> GetAvailableSlots(
        int dentistId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var availableSlots = await _appointmentService.GetAvailableSlotsAsync(dentistId, startDate, endDate);
            var response = availableSlots.Select(slot => new AvailableSlotResponse
            {
                DentistId = dentistId,
                DentistName = "Dentist", // TODO: Get actual dentist name
                DateTime = slot,
                DurationMinutes = 30,
                IsAvailable = true
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting available slots for dentist {DentistId}", dentistId);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Gets available time slots for all dentists
    /// </summary>
    /// <param name="startDate">Start date for availability check</param>
    /// <param name="endDate">End date for availability check</param>
    /// <returns>List of available time slots</returns>
    [HttpGet("available-slots")]
    [ProducesResponseType(typeof(IEnumerable<AvailableSlotResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AvailableSlotResponse>>> GetAllAvailableSlots(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            if (startDate >= endDate)
            {
                return BadRequest("Start date must be before end date");
            }

            var allAvailableSlots = await _appointmentService.GetAllAvailableSlotsAsync(startDate, endDate);
            var response = allAvailableSlots.SelectMany(kvp =>
                kvp.Value.Select(slot => new AvailableSlotResponse
                {
                    DentistId = kvp.Key,
                    DentistName = "Dentist", // TODO: Get actual dentist name
                    DateTime = slot,
                    DurationMinutes = 30,
                    IsAvailable = true
                }));

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all available slots");
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Creates a new appointment
    /// </summary>
    /// <param name="request">Appointment creation request</param>
    /// <returns>Created appointment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppointmentResponse>> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the slot is available
            var isAvailable = await _appointmentService.IsSlotAvailableAsync(request.DentistId, request.AppointmentDateTime);
            if (!isAvailable)
            {
                return BadRequest("The selected time slot is not available");
            }

            var appointment = await _appointmentService.CreateAppointmentAsync(
                request.CustomerId,
                request.DentistId,
                request.AppointmentDateTime,
                request.ProcedureType,
                request.Notes);

            var response = MapToResponse(appointment);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for appointment creation");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating appointment");
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Updates an existing appointment
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="request">Appointment update request</param>
    /// <returns>Updated appointment</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointment(int id, [FromBody] UpdateAppointmentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(
                id,
                request.CustomerId,
                request.DentistId,
                request.AppointmentDateTime,
                request.ProcedureType,
                request.Notes);

            if (updatedAppointment == null)
            {
                return NotFound($"Appointment with ID {id} not found");
            }

            return Ok(MapToResponse(updatedAppointment));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for appointment update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating appointment {AppointmentId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Updates the status of an appointment
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Updated appointment</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(AppointmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AppointmentResponse>> UpdateAppointmentStatus(int id, [FromBody] UpdateAppointmentStatusRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedAppointment = await _appointmentService.UpdateAppointmentStatusAsync(id, request.Status, request.Reason);
            if (updatedAppointment == null)
            {
                return NotFound($"Appointment with ID {id} not found");
            }

            return Ok(MapToResponse(updatedAppointment));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for appointment status update");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating appointment status {AppointmentId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Deletes an appointment
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAppointment(int id)
    {
        try
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);
            if (!result)
            {
                return NotFound($"Appointment with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting appointment {AppointmentId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    private static AppointmentResponse MapToResponse(Appointment appointment)
    {
        return new AppointmentResponse
        {
            Id = appointment.Id,
            CustomerId = appointment.CustomerId,
            CustomerName = appointment.Customer?.Name ?? "Unknown",
            DentistId = appointment.DentistId,
            DentistName = appointment.Dentist?.Name ?? "Unknown",
            AppointmentDateTime = appointment.AppointmentDateTime,
            ProcedureType = appointment.ProcedureType,
            Notes = appointment.Notes,
            Status = appointment.Status.GetDescription(),
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}
