using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Application.Services;

/// <summary>
/// Service for customer management operations
/// </summary>
public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuthApiService _authApiService;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the CustomerService
    /// </summary>
    /// <param name="customerRepository">Customer repository</param>
    /// <param name="authApiService">Auth API service</param>
    /// <param name="emailService">Email service</param>
    public CustomerService(
        ICustomerRepository customerRepository,
        IAuthApiService authApiService,
        IEmailService emailService)
    {
        _customerRepository = customerRepository;
        _authApiService = authApiService;
        _emailService = emailService;
    }

    /// <summary>
    /// Creates a new customer and associated user account
    /// </summary>
    /// <param name="customer">Customer to create</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message, Customer? Customer)> CreateCustomerAsync(Customer customer)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(customer.Name) || 
            string.IsNullOrWhiteSpace(customer.Email) ||
            string.IsNullOrWhiteSpace(customer.Phone))
        {
            return (false, "Name, email and phone are required.", null);
        }

        // Check if email already exists in Auth API
        var userExists = await _authApiService.UserExistsByEmailAsync(customer.Email);
        if (userExists)
        {
            return (false, "A user with this email already exists in the system.", null);
        }

        // Check if customer already exists
        var existingCustomer = await _customerRepository.GetByEmailAsync(customer.Email);
        if (existingCustomer != null)
        {
            return (false, "A customer with this email already exists.", null);
        }

        try
        {
            // Generate auto password
            var password = GeneratePassword();
            var username = GenerateUsername(customer.Email);

            // Create user in Auth API
            var createUserRequest = new CreateUserRequest
            {
                Username = username,
                Email = customer.Email,
                Password = password,
                Role = "Customer"
            };

            var userCreated = await _authApiService.CreateUserAsync(createUserRequest);
            if (!userCreated)
            {
                return (false, "Failed to create user account.", null);
            }

            // Create customer
            var createdCustomer = await _customerRepository.CreateAsync(customer);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(
                customer.Email, 
                customer.Name, 
                username, 
                password, 
                "Customer");

            return (true, "Customer created successfully.", createdCustomer);
        }
        catch (Exception ex)
        {
            // If customer creation fails, try to clean up the user account
            await _authApiService.DeleteUserByEmailAsync(customer.Email);
            return (false, $"Failed to create customer: {ex.Message}", null);
        }
    }

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Customer if found, null otherwise</returns>
    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Gets all customers with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of customers</returns>
    public async Task<IEnumerable<Customer>> GetAllCustomersAsync(int page = 1, int pageSize = 10)
    {
        return await _customerRepository.GetAllAsync(page, pageSize);
    }

    /// <summary>
    /// Updates a customer
    /// </summary>
    /// <param name="customer">Customer to update</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> UpdateCustomerAsync(Customer customer)
    {
        if (customer.Id <= 0)
        {
            return (false, "Invalid customer ID.");
        }

        var existingCustomer = await _customerRepository.GetByIdAsync(customer.Id);
        if (existingCustomer == null)
        {
            return (false, "Customer not found.");
        }

        customer.UpdatedAt = DateTime.UtcNow;
        await _customerRepository.UpdateAsync(customer);

        return (true, "Customer updated successfully.");
    }

    /// <summary>
    /// Deletes a customer and associated user account
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> DeleteCustomerAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return (false, "Customer not found.");
        }

        // Delete customer
        var deleted = await _customerRepository.DeleteAsync(id);
        if (!deleted)
        {
            return (false, "Failed to delete customer.");
        }

        // Delete user from Auth API
        await _authApiService.DeleteUserByEmailAsync(customer.Email);

        return (true, "Customer deleted successfully.");
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
