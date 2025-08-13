using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Services;

/// <summary>
/// Service for dentist management operations
/// </summary>
public class DentistService
{
    private readonly IDentistRepository _dentistRepository;
    private readonly IAuthApiService _authApiService;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the DentistService
    /// </summary>
    /// <param name="dentistRepository">Dentist repository</param>
    /// <param name="authApiService">Auth API service</param>
    /// <param name="emailService">Email service</param>
    public DentistService(
        IDentistRepository dentistRepository,
        IAuthApiService authApiService,
        IEmailService emailService)
    {
        _dentistRepository = dentistRepository;
        _authApiService = authApiService;
        _emailService = emailService;
    }

    /// <summary>
    /// Creates a new dentist and associated user account
    /// </summary>
    /// <param name="dentist">Dentist to create</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message, Dentist? Dentist)> CreateDentistAsync(Dentist dentist)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dentist.Name) || 
            string.IsNullOrWhiteSpace(dentist.Email) ||
            string.IsNullOrWhiteSpace(dentist.Phone) ||
            string.IsNullOrWhiteSpace(dentist.LicenseNumber))
        {
            return (false, "Name, email, phone and license number are required.", null);
        }

        // Check if email already exists in Auth API
        var userExists = await _authApiService.UserExistsByEmailAsync(dentist.Email);
        if (userExists)
        {
            return (false, "A user with this email already exists in the system.", null);
        }

        // Check if dentist already exists
        var existingDentist = await _dentistRepository.GetByEmailAsync(dentist.Email);
        if (existingDentist != null)
        {
            return (false, "A dentist with this email already exists.", null);
        }

        try
        {
            // Generate auto password
            var password = GeneratePassword();
            var username = GenerateUsername(dentist.Email);

            // Create user in Auth API
            var createUserRequest = new CreateUserRequest
            {
                FirstName = dentist.Name.Split(' ').FirstOrDefault() ?? dentist.Name,
                LastName = dentist.Name.Split(' ').Skip(1).FirstOrDefault() ?? "",
                Email = dentist.Email,
                Password = password,
                Role = "Dentist"
            };

            var userCreated = await _authApiService.CreateUserAsync(createUserRequest);
            if (!userCreated)
            {
                return (false, "Failed to create user account.", null);
            }

            // Create dentist
            var createdDentist = await _dentistRepository.CreateAsync(dentist);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(
                dentist.Email, 
                dentist.Name, 
                username, 
                password, 
                "Dentist");

            return (true, "Dentist created successfully.", createdDentist);
        }
        catch (Exception ex)
        {
            // If dentist creation fails, try to clean up the user account
            await _authApiService.DeleteUserByEmailAsync(dentist.Email);
            return (false, $"Failed to create dentist: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Gets a dentist by ID
    /// </summary>
    /// <param name="id">Dentist ID</param>
    /// <returns>Dentist if found, null otherwise</returns>
    public async Task<Dentist?> GetDentistByIdAsync(int id)
    {
        return await _dentistRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets all dentists with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of dentists</returns>
    public async Task<IEnumerable<Dentist>> GetAllDentistsAsync(int page = 1, int pageSize = 10)
    {
        return await _dentistRepository.GetAllAsync(page, pageSize);
    }

    /// <summary>
    /// Updates a dentist
    /// </summary>
    /// <param name="dentist">Dentist to update</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> UpdateDentistAsync(Dentist dentist)
    {
        if (dentist.Id <= 0)
        {
            return (false, "Invalid dentist ID.");
        }

        var existingDentist = await _dentistRepository.GetByIdAsync(dentist.Id);
        if (existingDentist == null)
        {
            return (false, "Dentist not found.");
        }

        dentist.UpdatedAt = DateTime.UtcNow;
        await _dentistRepository.UpdateAsync(dentist);

        return (true, "Dentist updated successfully.");
    }

    /// <summary>
    /// Deletes a dentist and associated user account
    /// </summary>
    /// <param name="id">Dentist ID</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> DeleteDentistAsync(int id)
    {
        var dentist = await _dentistRepository.GetByIdAsync(id);
        if (dentist == null)
        {
            return (false, "Dentist not found.");
        }

        // Delete dentist
        var deleted = await _dentistRepository.DeleteAsync(id);
        if (!deleted)
        {
            return (false, "Failed to delete dentist.");
        }

        // Delete user from Auth API
        await _authApiService.DeleteUserByEmailAsync(dentist.Email);

        return (true, "Dentist deleted successfully.");
    }

    /// <summary>
    /// Generates a random password
    /// </summary>
    /// <returns>Generated password</returns>
    private static string GeneratePassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Generates a username from email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>Generated username</returns>
    private static string GenerateUsername(string email)
    {
        var localPart = email.Split('@')[0];
        return $"{localPart}_{DateTime.UtcNow.Ticks % 10000}";
    }
}
