using Microsoft.AspNetCore.Mvc;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.DTOs;
using NiceDentist.Manager.Api.DTOs.Responses;

namespace NiceDentist.Manager.Api.Controllers;

/// <summary>
/// Controller for managing dentists
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DentistsController : ControllerBase
{
    private const string GenericErrorMessage = "An error occurred while processing your request";
    
    private readonly IDentistService _dentistService;
    private readonly ILogger<DentistsController> _logger;

    /// <summary>
    /// Initializes a new instance of the DentistsController
    /// </summary>
    /// <param name="dentistService">The dentist service</param>
    /// <param name="logger">The logger</param>
    public DentistsController(IDentistService dentistService, ILogger<DentistsController> logger)
    {
        _dentistService = dentistService ?? throw new ArgumentNullException(nameof(dentistService));
        _logger = logger;
    }

    /// <summary>
    /// Gets all dentists with pagination and search
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="search">Optional search term</param>
    /// <returns>A paged list of dentists</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DentistResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<DentistResponse>>> GetAllDentists(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        try
        {
            var result = await _dentistService.GetAllDentistsAsync(page, pageSize, search);
            var response = new PagedResponse<DentistResponse>
            {
                Data = result.Items.Select(MapToResponse),
                Page = result.Page,
                PageSize = result.PageSize,
                Total = result.TotalCount,
                TotalPages = result.TotalPages
            };
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting dentists");
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Gets a dentist by ID
    /// </summary>
    /// <param name="id">The dentist ID</param>
    /// <returns>The dentist if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DentistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DentistResponse>> GetDentist(int id)
    {
        try
        {
            var dentist = await _dentistService.GetDentistByIdAsync(id);
            if (dentist == null)
            {
                return NotFound($"Dentist with ID {id} not found");
            }

            return Ok(MapToResponse(dentist));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting dentist {DentistId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Gets a dentist by email
    /// </summary>
    /// <param name="email">The dentist email</param>
    /// <returns>The dentist if found</returns>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(DentistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DentistResponse>> GetDentistByEmail(string email)
    {
        try
        {
            var dentist = await _dentistService.GetDentistByEmailAsync(email);
            if (dentist == null)
            {
                return NotFound($"Dentist with email {email} not found");
            }

            return Ok(MapToResponse(dentist));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting dentist by email {Email}", email);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Creates a new dentist with Auth API integration
    /// </summary>
    /// <param name="dentistDto">The dentist data</param>
    /// <returns>The created dentist</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DentistResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DentistResponse>> CreateDentist([FromBody] DentistDto dentistDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Use the method that publishes events for Auth API integration
            var createdDentist = await _dentistService.CreateDentistWithAuthAsync(dentistDto);
            var response = MapToResponse(createdDentist);

            return CreatedAtAction(nameof(GetDentist), new { id = response.Id }, response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning(ex, "Duplicate dentist email: {Email}", dentistDto.Email);
            return Conflict(new { 
                message = ex.Message,
                field = "email",
                code = "DUPLICATE_EMAIL"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid dentist data provided");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating dentist");
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Updates an existing dentist
    /// </summary>
    /// <param name="id">The dentist ID</param>
    /// <param name="dentistDto">The updated dentist data</param>
    /// <returns>The updated dentist</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DentistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DentistResponse>> UpdateDentist(int id, [FromBody] DentistDto dentistDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedDentist = await _dentistService.UpdateDentistAsync(id, dentistDto);
            return Ok(MapToResponse(updatedDentist));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid dentist data provided for dentist {DentistId}", id);
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict while updating dentist {DentistId}", id);
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating dentist {DentistId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Deletes a dentist
    /// </summary>
    /// <param name="id">The dentist ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDentist(int id)
    {
        try
        {
            var deleted = await _dentistService.DeleteDentistAsync(id);
            if (!deleted)
            {
                return NotFound($"Dentist with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting dentist {DentistId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Maps a DentistDto to a DentistResponse
    /// </summary>
    /// <param name="dentistDto">The dentist DTO</param>
    /// <returns>The dentist response</returns>
    private static DentistResponse MapToResponse(DentistDto dentistDto)
    {
        return new DentistResponse
        {
            Id = dentistDto.Id,
            Name = dentistDto.Name,
            Email = dentistDto.Email,
            Phone = dentistDto.Phone,
            LicenseNumber = dentistDto.LicenseNumber,
            Specialization = dentistDto.Specialization,
            CreatedAt = dentistDto.CreatedAt,
            UpdatedAt = dentistDto.UpdatedAt,
            IsActive = dentistDto.IsActive,
            UserId = dentistDto.UserId
        };
    }
}
