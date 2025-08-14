using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of ICustomerRepository for testing and development
/// </summary>
public class InMemoryCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new();
    private int _nextId = 1;

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="customer">The customer to create</param>
    /// <returns>The created customer with assigned ID</returns>
    public Task<Customer> CreateAsync(Customer customer)
    {
        customer.Id = _nextId++;
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;
        _customers.Add(customer);
        return Task.FromResult(customer);
    }

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>The customer if found, null otherwise</returns>
    public Task<Customer?> GetByIdAsync(int id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(customer);
    }

    /// <summary>
    /// Gets a customer by email
    /// </summary>
    /// <param name="email">The customer email</param>
    /// <returns>The customer if found, null otherwise</returns>
    public Task<Customer?> GetByEmailAsync(string email)
    {
        var customer = _customers.FirstOrDefault(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(customer);
    }

    /// <summary>
    /// Gets all customers with pagination and optional search
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Optional search term</param>
    /// <returns>List of customers</returns>
    public Task<IEnumerable<Customer>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => 
                c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(c.Phone) && c.Phone.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var customers = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<IEnumerable<Customer>>(customers);
    }

    /// <summary>
    /// Gets the total count of customers with optional search
    /// NOTA: In-memory repository - apenas para testes
    /// </summary>
    /// <param name="search">Optional search term</param>
    /// <returns>Total count of customers</returns>
    public Task<int> GetCountAsync(string? search = null)
    {
        var query = _customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => 
                c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(c.Phone) && c.Phone.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        return Task.FromResult(query.Count());
    }

    /// <summary>
    /// Updates a customer
    /// </summary>
    /// <param name="customer">The customer to update</param>
    /// <returns>The updated customer</returns>
    public Task<Customer> UpdateAsync(Customer customer)
    {
        var existingCustomer = _customers.FirstOrDefault(c => c.Id == customer.Id);
        if (existingCustomer != null)
        {
            var index = _customers.IndexOf(existingCustomer);
            customer.UpdatedAt = DateTime.UtcNow;
            _customers[index] = customer;
            return Task.FromResult(customer);
        }

        throw new InvalidOperationException($"Customer with ID {customer.Id} not found");
    }

    /// <summary>
    /// Deletes a customer
    /// </summary>
    /// <param name="id">The customer ID to delete</param>
    /// <returns>True if deleted, false otherwise</returns>
    public Task<bool> DeleteAsync(int id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        if (customer != null)
        {
            _customers.Remove(customer);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}
