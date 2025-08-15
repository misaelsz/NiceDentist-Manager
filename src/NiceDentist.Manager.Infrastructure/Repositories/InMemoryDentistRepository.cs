using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of dentist repository for testing
/// </summary>
public class InMemoryDentistRepository : IDentistRepository
{
    private readonly List<Dentist> _dentists = new();
    private int _nextId = 1;

    /// <summary>
    /// Gets all dentists with pagination
    /// </summary>
    public Task<IEnumerable<Dentist>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        var skipCount = (page - 1) * pageSize;
        var paginatedDentists = _dentists.Skip(skipCount).Take(pageSize);
        return Task.FromResult<IEnumerable<Dentist>>(paginatedDentists.ToList());
    }

    /// <summary>
    /// Gets a dentist by ID
    /// </summary>
    public Task<Dentist?> GetByIdAsync(int id)
    {
        var dentist = _dentists.FirstOrDefault(d => d.Id == id);
        return Task.FromResult(dentist);
    }

    /// <summary>
    /// Gets a dentist by email
    /// </summary>
    public Task<Dentist?> GetByEmailAsync(string email)
    {
        var dentist = _dentists.FirstOrDefault(d => d.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(dentist);
    }

    /// <summary>
    /// Gets a dentist by license number
    /// </summary>
    public Task<Dentist?> GetByLicenseNumberAsync(string licenseNumber)
    {
        var dentist = _dentists.FirstOrDefault(d => d.LicenseNumber.Equals(licenseNumber, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(dentist);
    }

    /// <summary>
    /// Creates a new dentist
    /// </summary>
    public Task<Dentist> CreateAsync(Dentist dentist)
    {
        dentist.Id = _nextId++;
        dentist.CreatedAt = DateTime.UtcNow;
        dentist.UpdatedAt = DateTime.UtcNow;
        _dentists.Add(dentist);
        return Task.FromResult(dentist);
    }

    /// <summary>
    /// Updates an existing dentist
    /// </summary>
    public Task<Dentist> UpdateAsync(Dentist dentist)
    {
        var existingDentist = _dentists.FirstOrDefault(d => d.Id == dentist.Id);
        if (existingDentist == null)
        {
            throw new ArgumentException($"Dentist with ID {dentist.Id} not found", nameof(dentist));
        }

        var index = _dentists.IndexOf(existingDentist);
        dentist.UpdatedAt = DateTime.UtcNow;
        _dentists[index] = dentist;
        
        return Task.FromResult(dentist);
    }

    /// <summary>
    /// Deletes a dentist by ID
    /// </summary>
    public Task<bool> DeleteAsync(int id)
    {
        var dentist = _dentists.FirstOrDefault(d => d.Id == id);
        if (dentist == null)
        {
            return Task.FromResult(false);
        }

        _dentists.Remove(dentist);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Updates only the UserId field for a dentist (used by events)
    /// </summary>
    /// <param name="dentistId">The dentist ID</param>
    /// <param name="userId">The user ID to set</param>
    /// <returns>True if updated successfully</returns>
    public Task<bool> UpdateUserIdAsync(int dentistId, int userId)
    {
        var dentist = _dentists.FirstOrDefault(d => d.Id == dentistId);
        if (dentist != null)
        {
            dentist.UserId = userId;
            dentist.UpdatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }
}
