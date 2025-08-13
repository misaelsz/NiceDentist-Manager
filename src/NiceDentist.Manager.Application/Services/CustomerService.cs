namespace NiceDentist.Manager.Application.Services;

/// <summary>
/// Service for customer management operations
/// </summary>
public class CustomerService : ICustomerService
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
    public async Task<(bool Success, string Message, Customer? Customer)> CreateCustomerWithAuthAsync(Customer customer)
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
                FirstName = customer.Name.Split(' ').FirstOrDefault() ?? customer.Name,
                LastName = customer.Name.Split(' ').Skip(1).FirstOrDefault() ?? "",
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
    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer != null ? MapToDto(customer) : null;
    }

    /// <summary>
    /// Gets all customers with pagination and search
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="search">Optional search term</param>
    /// <returns>List of customers</returns>
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        var customers = await _customerRepository.GetAllAsync(page, pageSize, search);
        return customers.Select(MapToDto);
    }

    /// <summary>
    /// Updates a customer
    /// </summary>
    /// <param name="customer">Customer to update</param>
    /// <returns>Result with success status and message</returns>
    public async Task<(bool Success, string Message)> UpdateCustomerWithAuthAsync(Customer customer)
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
    public async Task<(bool Success, string Message)> DeleteCustomerWithAuthAsync(int id)
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

    /// <summary>
    /// Maps a Customer entity to a CustomerDto
    /// </summary>
    /// <param name="customer">The customer entity</param>
    /// <returns>The customer DTO</returns>
    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = string.IsNullOrEmpty(customer.Phone) ? null : customer.Phone,
            DateOfBirth = customer.DateOfBirth == default ? null : customer.DateOfBirth,
            Address = string.IsNullOrEmpty(customer.Address) ? null : customer.Address,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            IsActive = customer.IsActive
        };
    }

    /// <summary>
    /// Maps a CustomerDto to a Customer entity
    /// </summary>
    /// <param name="customerDto">The customer DTO</param>
    /// <returns>The customer entity</returns>
    private static Customer MapToEntity(CustomerDto customerDto)
    {
        return new Customer
        {
            Id = customerDto.Id,
            Name = customerDto.Name,
            Email = customerDto.Email,
            Phone = customerDto.Phone ?? string.Empty,
            DateOfBirth = customerDto.DateOfBirth ?? default,
            Address = customerDto.Address ?? string.Empty,
            CreatedAt = customerDto.CreatedAt,
            UpdatedAt = customerDto.UpdatedAt,
            IsActive = customerDto.IsActive
        };
    }

    // ICustomerService interface implementations
    /// <summary>
    /// Creates a new customer (simple DTO-based version)
    /// </summary>
    /// <param name="customerDto">Customer DTO</param>
    /// <returns>Created customer DTO</returns>
    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto)
    {
        var customer = MapToEntity(customerDto);
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;
        customer.IsActive = true;

        var createdCustomer = await _customerRepository.CreateAsync(customer);
        return MapToDto(createdCustomer);
    }

    /// <summary>
    /// Updates an existing customer (simple DTO-based version)
    /// </summary>
    /// <param name="customerDto">Customer DTO with updates</param>
    /// <returns>Updated customer DTO if found</returns>
    public async Task<CustomerDto?> UpdateCustomerAsync(CustomerDto customerDto)
    {
        var existingCustomer = await _customerRepository.GetByIdAsync(customerDto.Id);
        if (existingCustomer == null)
        {
            return null;
        }

        existingCustomer.Name = customerDto.Name;
        existingCustomer.Email = customerDto.Email;
        existingCustomer.Phone = customerDto.Phone ?? string.Empty;
        existingCustomer.DateOfBirth = customerDto.DateOfBirth ?? default;
        existingCustomer.Address = customerDto.Address ?? string.Empty;
        existingCustomer.UpdatedAt = DateTime.UtcNow;

        var updatedCustomer = await _customerRepository.UpdateAsync(existingCustomer);
        return updatedCustomer != null ? MapToDto(updatedCustomer) : null;
    }

    /// <summary>
    /// Deletes a customer (simple version)
    /// </summary>
    /// <param name="id">Customer ID</param>
    /// <returns>True if deleted successfully</returns>
    public async Task<bool> DeleteCustomerAsync(int id)
    {
        return await _customerRepository.DeleteAsync(id);
    }
}
