using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Repository interface for Customer operations
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="customer">The customer to create</param>
    /// <returns>The created customer with assigned ID</returns>
    Task<Customer> CreateAsync(Customer customer);

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>The customer if found, null otherwise</returns>
    Task<Customer?> GetByIdAsync(int id);

    /// <summary>
    /// Gets a customer by email
    /// </summary>
    /// <param name="email">The customer email</param>
    /// <returns>The customer if found, null otherwise</returns>
    Task<Customer?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets all customers with pagination and optional search
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Optional search term</param>
    /// <returns>List of customers</returns>
    Task<IEnumerable<Customer>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null);

    /// <summary>
    /// Gets the total count of customers with optional search
    /// </summary>
    /// <param name="search">Optional search term</param>
    /// <returns>Total count of customers</returns>
    Task<int> GetCountAsync(string? search = null);

    /// <summary>
    /// Updates a customer
    /// </summary>
    /// <param name="customer">The customer to update</param>
    /// <returns>The updated customer</returns>
    Task<Customer> UpdateAsync(Customer customer);

    /// <summary>
    /// Updates only the UserId field for a customer (used by events)
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="userId">The user ID to set</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateUserIdAsync(int customerId, int userId);

    /// <summary>
    /// Deletes a customer
    /// </summary>
    /// <param name="id">The customer ID to delete</param>
    /// <returns>True if deleted, false otherwise</returns>
    Task<bool> DeleteAsync(int id);
}
