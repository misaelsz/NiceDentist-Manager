using Microsoft.AspNetCore.Mvc;
using NiceDentist.Manager.Api.DTOs.Requests;
using NiceDentist.Manager.Api.DTOs.Responses;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.DTOs;

namespace NiceDentist.Manager.Api.Controllers;

/// <summary>
/// Controller for managing customers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private const string GenericErrorMessage = "An error occurred while processing your request";
    
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    /// <summary>
    /// Initializes a new instance of the CustomersController
    /// </summary>
    /// <param name="customerService">The customer service</param>
    /// <param name="logger">The logger</param>
    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all customers
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="search">Optional search term</param>
    /// <returns>A list of customers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetAllCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        try
        {
            var customers = await _customerService.GetAllCustomersAsync(page, pageSize, search);
            var response = customers.Select(MapToResponse);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting customers");
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>The customer if found</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetCustomerById(int id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return Ok(MapToResponse(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting customer {CustomerId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="request">The customer creation request</param>
    /// <returns>The created customer</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerDto = new CustomerDto
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address
            };

            var createdCustomer = await _customerService.CreateCustomerAsync(customerDto);
            var response = MapToResponse(createdCustomer);

            return CreatedAtAction(nameof(GetCustomerById), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid customer data provided");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating customer");
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <param name="request">The customer update request</param>
    /// <returns>The updated customer</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customerDto = new CustomerDto
            {
                Id = id,
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address
            };

            var updatedCustomer = await _customerService.UpdateCustomerAsync(customerDto);
            if (updatedCustomer == null)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return Ok(MapToResponse(updatedCustomer));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid customer data provided for customer {CustomerId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating customer {CustomerId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Deletes a customer
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        try
        {
            var success = await _customerService.DeleteCustomerAsync(id);
            if (!success)
            {
                return NotFound($"Customer with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting customer {CustomerId}", id);
            return StatusCode(500, GenericErrorMessage);
        }
    }

    /// <summary>
    /// Maps a CustomerDto to a CustomerResponse
    /// </summary>
    /// <param name="customerDto">The customer DTO</param>
    /// <returns>The customer response</returns>
    private static CustomerResponse MapToResponse(CustomerDto customerDto)
    {
        return new CustomerResponse
        {
            Id = customerDto.Id,
            Name = customerDto.Name,
            Email = customerDto.Email,
            Phone = customerDto.Phone,
            DateOfBirth = customerDto.DateOfBirth,
            Address = customerDto.Address,
            CreatedAt = customerDto.CreatedAt,
            UpdatedAt = customerDto.UpdatedAt,
            IsActive = customerDto.IsActive
        };
    }
}
