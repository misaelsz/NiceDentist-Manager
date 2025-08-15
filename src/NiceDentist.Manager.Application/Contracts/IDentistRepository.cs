using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Repository interface for Dentist operations
/// </summary>
public interface IDentistRepository
{
    /// <summary>
    /// Creates a new dentist
    /// </summary>
    /// <param name="dentist">The dentist to create</param>
    /// <returns>The created dentist with assigned ID</returns>
    Task<Dentist> CreateAsync(Dentist dentist);

    /// <summary>
    /// Gets a dentist by ID
    /// </summary>
    /// <param name="id">The dentist ID</param>
    /// <returns>The dentist if found, null otherwise</returns>
    Task<Dentist?> GetByIdAsync(int id);

    /// <summary>
    /// Gets a dentist by email
    /// </summary>
    /// <param name="email">The dentist email</param>
    /// <returns>The dentist if found, null otherwise</returns>
    Task<Dentist?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets a dentist by license number
    /// </summary>
    /// <param name="licenseNumber">The dentist license number</param>
    /// <returns>The dentist if found, null otherwise</returns>
    Task<Dentist?> GetByLicenseNumberAsync(string licenseNumber);

    /// <summary>
    /// Gets all dentists with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>List of dentists</returns>
    Task<IEnumerable<Dentist>> GetAllAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Updates a dentist
    /// </summary>
    /// <param name="dentist">The dentist to update</param>
    /// <returns>The updated dentist</returns>
    Task<Dentist> UpdateAsync(Dentist dentist);

    /// <summary>
    /// Deletes a dentist
    /// </summary>
    /// <param name="id">The dentist ID to delete</param>
    /// <returns>True if deleted, false otherwise</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Updates only the UserId field for a dentist (used by events)
    /// </summary>
    /// <param name="dentistId">The dentist ID</param>
    /// <param name="userId">The user ID to set</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateUserIdAsync(int dentistId, int userId);
}
