using NiceDentist.Manager.Application.DTOs;

namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Interface for dentist service operations
/// </summary>
public interface IDentistService
{
    /// <summary>
    /// Gets all dentists with pagination
    /// </summary>
    Task<IEnumerable<DentistDto>> GetAllDentistsAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Gets a dentist by ID
    /// </summary>
    Task<DentistDto?> GetDentistByIdAsync(int id);

    /// <summary>
    /// Gets a dentist by email
    /// </summary>
    Task<DentistDto?> GetDentistByEmailAsync(string email);

    /// <summary>
    /// Creates a new dentist
    /// </summary>
    Task<DentistDto> CreateDentistAsync(DentistDto dentistDto);

    /// <summary>
    /// Creates a new dentist with Auth API integration (publishes event)
    /// </summary>
    Task<DentistDto> CreateDentistWithAuthAsync(DentistDto dentistDto);

    /// <summary>
    /// Updates an existing dentist
    /// </summary>
    Task<DentistDto> UpdateDentistAsync(int id, DentistDto dentistDto);

    /// <summary>
    /// Deletes a dentist
    /// </summary>
    Task<bool> DeleteDentistAsync(int id);
}
