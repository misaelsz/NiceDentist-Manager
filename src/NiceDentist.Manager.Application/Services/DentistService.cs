using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.DTOs;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Services;

/// <summary>
/// Service for managing dentists
/// </summary>
public class DentistService : IDentistService
{
    private readonly IDentistRepository _dentistRepository;

    public DentistService(IDentistRepository dentistRepository)
    {
        _dentistRepository = dentistRepository ?? throw new ArgumentNullException(nameof(dentistRepository));
    }

    /// <summary>
    /// Gets all dentists with pagination
    /// </summary>
    public async Task<IEnumerable<DentistDto>> GetAllDentistsAsync(int page = 1, int pageSize = 10)
    {
        var dentists = await _dentistRepository.GetAllAsync(page, pageSize);
        return dentists.Select(MapToDto);
    }

    /// <summary>
    /// Gets a dentist by ID
    /// </summary>
    public async Task<DentistDto?> GetDentistByIdAsync(int id)
    {
        var dentist = await _dentistRepository.GetByIdAsync(id);
        return dentist != null ? MapToDto(dentist) : null;
    }

    /// <summary>
    /// Gets a dentist by email
    /// </summary>
    public async Task<DentistDto?> GetDentistByEmailAsync(string email)
    {
        var dentist = await _dentistRepository.GetByEmailAsync(email);
        return dentist != null ? MapToDto(dentist) : null;
    }

    /// <summary>
    /// Creates a new dentist
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when email already exists</exception>
    public async Task<DentistDto> CreateDentistAsync(DentistDto dentistDto)
    {
        // Check if email already exists
        var existingDentist = await _dentistRepository.GetByEmailAsync(dentistDto.Email);
        if (existingDentist != null)
        {
            throw new InvalidOperationException($"A dentist with email '{dentistDto.Email}' already exists.");
        }

        var dentist = MapToEntity(dentistDto);
        dentist.CreatedAt = DateTime.UtcNow;
        dentist.UpdatedAt = DateTime.UtcNow;
        dentist.IsActive = true;

        var createdDentist = await _dentistRepository.CreateAsync(dentist);
        return MapToDto(createdDentist);
    }

    /// <summary>
    /// Updates an existing dentist
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when dentist not found or email already exists</exception>
    public async Task<DentistDto> UpdateDentistAsync(int id, DentistDto dentistDto)
    {
        var existingDentist = await _dentistRepository.GetByIdAsync(id);
        if (existingDentist == null)
        {
            throw new InvalidOperationException($"Dentist with ID {id} not found.");
        }

        // Check if email is being changed and if it already exists
        if (existingDentist.Email != dentistDto.Email)
        {
            var dentistWithSameEmail = await _dentistRepository.GetByEmailAsync(dentistDto.Email);
            if (dentistWithSameEmail != null)
            {
                throw new InvalidOperationException($"A dentist with email '{dentistDto.Email}' already exists.");
            }
        }

        // Update the dentist
        existingDentist.Name = dentistDto.Name;
        existingDentist.Email = dentistDto.Email;
        existingDentist.Phone = dentistDto.Phone;
        existingDentist.LicenseNumber = dentistDto.LicenseNumber;
        existingDentist.Specialization = dentistDto.Specialization;
        existingDentist.UpdatedAt = DateTime.UtcNow;
        existingDentist.IsActive = dentistDto.IsActive;

        var updatedDentist = await _dentistRepository.UpdateAsync(existingDentist);
        return MapToDto(updatedDentist);
    }

    /// <summary>
    /// Deletes a dentist
    /// </summary>
    public async Task<bool> DeleteDentistAsync(int id)
    {
        return await _dentistRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Maps a Dentist entity to DTO
    /// </summary>
    private static DentistDto MapToDto(Dentist dentist)
    {
        return new DentistDto
        {
            Id = dentist.Id,
            Name = dentist.Name,
            Email = dentist.Email,
            Phone = dentist.Phone,
            LicenseNumber = dentist.LicenseNumber,
            Specialization = dentist.Specialization,
            CreatedAt = dentist.CreatedAt,
            UpdatedAt = dentist.UpdatedAt,
            IsActive = dentist.IsActive,
            UserId = dentist.UserId
        };
    }

    /// <summary>
    /// Maps a DentistDto to entity
    /// </summary>
    private static Dentist MapToEntity(DentistDto dto)
    {
        return new Dentist
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            LicenseNumber = dto.LicenseNumber,
            Specialization = dto.Specialization,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            IsActive = dto.IsActive,
            UserId = dto.UserId
        };
    }
}
