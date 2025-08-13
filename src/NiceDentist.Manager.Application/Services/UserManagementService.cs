using NiceDentist.Manager.Application.Contracts;

namespace NiceDentist.Manager.Application.Services;

/// <summary>
/// Service for user management operations (Admin only)
/// </summary>
public class UserManagementService
{
    private readonly IAuthApiService _authApiService;
    private readonly ICustomerRepository _customerRepository;
    private readonly IDentistRepository _dentistRepository;

    /// <summary>
    /// Initializes a new instance of the UserManagementService
    /// </summary>
    /// <param name="authApiService">Auth API service</param>
    /// <param name="customerRepository">Customer repository</param>
    /// <param name="dentistRepository">Dentist repository</param>
    public UserManagementService(
        IAuthApiService authApiService,
        ICustomerRepository customerRepository,
        IDentistRepository dentistRepository)
    {
        _authApiService = authApiService;
        _customerRepository = customerRepository;
        _dentistRepository = dentistRepository;
    }

    /// <summary>
    /// Permanently deletes a user by email (Admin only)
    /// </summary>
    /// <param name="email">Email of the user to delete</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> PermanentlyDeleteUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return (false, "Email is required.");
        }

        try
        {
            // Check if user exists in Auth API
            var userExists = await _authApiService.UserExistsByEmailAsync(email);
            if (!userExists)
            {
                return (false, "User not found in the authentication system.");
            }

            // Check if it's a customer and delete from customer table
            var customer = await _customerRepository.GetByEmailAsync(email);
            if (customer != null)
            {
                await _customerRepository.DeleteAsync(customer.Id);
            }

            // Check if it's a dentist and delete from dentist table
            var dentist = await _dentistRepository.GetByEmailAsync(email);
            if (dentist != null)
            {
                await _dentistRepository.DeleteAsync(dentist.Id);
            }

            // Delete from Auth API
            var deleted = await _authApiService.DeleteUserByEmailAsync(email);
            if (!deleted)
            {
                return (false, "Failed to delete user from authentication system.");
            }

            return (true, "User permanently deleted from all systems.");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to delete user: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if a user exists by email
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>True if user exists, false otherwise</returns>
    public async Task<bool> UserExistsAsync(string email)
    {
        return await _authApiService.UserExistsByEmailAsync(email);
    }
}
