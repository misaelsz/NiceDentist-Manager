using Microsoft.AspNetCore.Mvc;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.DTOs;

namespace NiceDentist.Manager.Api.Controllers;

/// <summary>
/// Controller for managing dentists
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DentistsController : ControllerBase
{
    private readonly IDentistService _dentistService;

    public DentistsController(IDentistService dentistService)
    {
        _dentistService = dentistService ?? throw new ArgumentNullException(nameof(dentistService));
    }

    /// <summary>
    /// Gets all dentists with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>List of dentists</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DentistDto>>> GetAllDentists([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var dentists = await _dentistService.GetAllDentistsAsync(page, pageSize);
        return Ok(dentists);
    }

    /// <summary>
    /// Gets a dentist by ID
    /// </summary>
    /// <param name="id">The dentist ID</param>
    /// <returns>The dentist if found</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<DentistDto>> GetDentist(int id)
    {
        var dentist = await _dentistService.GetDentistByIdAsync(id);
        if (dentist == null)
        {
            return NotFound($"Dentist with ID {id} not found");
        }

        return Ok(dentist);
    }

    /// <summary>
    /// Gets a dentist by email
    /// </summary>
    /// <param name="email">The dentist email</param>
    /// <returns>The dentist if found</returns>
    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<DentistDto>> GetDentistByEmail(string email)
    {
        var dentist = await _dentistService.GetDentistByEmailAsync(email);
        if (dentist == null)
        {
            return NotFound($"Dentist with email {email} not found");
        }

        return Ok(dentist);
    }

    /// <summary>
    /// Creates a new dentist with Auth API integration
    /// </summary>
    /// <param name="dentistDto">The dentist data</param>
    /// <returns>The created dentist</returns>
    [HttpPost]
    public async Task<ActionResult<DentistDto>> CreateDentist([FromBody] DentistDto dentistDto)
    {
        try
        {
            // Use the method that publishes events for Auth API integration
            var createdDentist = await _dentistService.CreateDentistWithAuthAsync(dentistDto);
            return CreatedAtAction(nameof(GetDentist), new { id = createdDentist.Id }, createdDentist);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the dentist", details = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing dentist
    /// </summary>
    /// <param name="id">The dentist ID</param>
    /// <param name="dentistDto">The updated dentist data</param>
    /// <returns>The updated dentist</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<DentistDto>> UpdateDentist(int id, [FromBody] DentistDto dentistDto)
    {
        try
        {
            var updatedDentist = await _dentistService.UpdateDentistAsync(id, dentistDto);
            return Ok(updatedDentist);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the dentist", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a dentist
    /// </summary>
    /// <param name="id">The dentist ID</param>
    /// <returns>Success message</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDentist(int id)
    {
        try
        {
            var deleted = await _dentistService.DeleteDentistAsync(id);
            if (!deleted)
            {
                return NotFound($"Dentist with ID {id} not found");
            }

            return Ok(new { message = "Dentist deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the dentist", details = ex.Message });
        }
    }
}
