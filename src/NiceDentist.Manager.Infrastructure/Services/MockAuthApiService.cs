using NiceDentist.Manager.Application.Contracts;

namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// Mock implementation of IAuthApiService for testing and development
/// </summary>
public class MockAuthApiService : IAuthApiService
{
    /// <summary>
    /// Checks if a user exists by email
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>Always returns false for mock</returns>
    public Task<bool> UserExistsByEmailAsync(string email)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Creates a user
    /// </summary>
    /// <param name="request">User creation request</param>
    /// <returns>Always returns true for mock</returns>
    public Task<bool> CreateUserAsync(CreateUserRequest request)
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Deletes a user by email
    /// </summary>
    /// <param name="email">Email of user to delete</param>
    /// <returns>Always returns true for mock</returns>
    public Task<bool> DeleteUserByEmailAsync(string email)
    {
        return Task.FromResult(true);
    }
}
