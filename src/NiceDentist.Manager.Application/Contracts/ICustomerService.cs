using NiceDentist.Manager.Application.DTOs;

namespace NiceDentist.Manager.Application.Contracts;

/// <summary>
/// Interface for customer service operations
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Gets all customers with optional pagination and search
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Optional search term</param>
    /// <returns>A list of customers</returns>
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(int page = 1, int pageSize = 10, string? search = null);

    /// <summary>
    /// Gets a customer by their unique identifier
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>The customer if found, null otherwise</returns>
    Task<CustomerDto?> GetCustomerByIdAsync(int id);

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="customerDto">The customer data</param>
    /// <returns>The created customer</returns>
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto);

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    /// <param name="customerDto">The customer data with updated information</param>
    /// <returns>The updated customer if found, null otherwise</returns>
    Task<CustomerDto?> UpdateCustomerAsync(CustomerDto customerDto);

    /// <summary>
    /// Deletes a customer by their unique identifier
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>True if the customer was deleted, false if not found</returns>
    Task<bool> DeleteCustomerAsync(int id);
}
