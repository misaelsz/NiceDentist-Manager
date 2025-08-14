using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NiceDentist.Manager.Api.Controllers;
using NiceDentist.Manager.Api.DTOs.Requests;
using NiceDentist.Manager.Api.DTOs.Responses;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Api;

/// <summary>
/// Unit tests for CustomersController
/// </summary>
public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _controller = new CustomersController(_mockCustomerService.Object);
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnOkWithCustomers_WhenCustomersExist()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer("John Doe", "john@email.com", "123-456-7890"),
            new Customer("Jane Smith", "jane@email.com", "987-654-3210")
        };
        _mockCustomerService.Setup(s => s.GetAllAsync()).ReturnsAsync(customers);

        // Act
        var result = await _controller.GetAllCustomers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<CustomerResponse[]>();
        var responseArray = okResult.Value as CustomerResponse[];
        responseArray.Should().HaveCount(2);
        responseArray![0].Name.Should().Be("John Doe");
        responseArray[1].Name.Should().Be("Jane Smith");
        
        _mockCustomerService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnOkWithEmptyArray_WhenNoCustomersExist()
    {
        // Arrange
        var customers = new List<Customer>();
        _mockCustomerService.Setup(s => s.GetAllAsync()).ReturnsAsync(customers);

        // Act
        var result = await _controller.GetAllCustomers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<CustomerResponse[]>();
        var responseArray = okResult.Value as CustomerResponse[];
        responseArray.Should().BeEmpty();
        
        _mockCustomerService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnOkWithCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer("John Doe", "john@email.com", "123-456-7890");
        _mockCustomerService.Setup(s => s.GetByIdAsync(customerId)).ReturnsAsync(customer);

        // Act
        var result = await _controller.GetCustomerById(customerId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<CustomerResponse>();
        var response = okResult.Value as CustomerResponse;
        response!.Name.Should().Be("John Doe");
        response.Email.Should().Be("john@email.com");
        response.Phone.Should().Be("123-456-7890");
        
        _mockCustomerService.Verify(s => s.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task GetCustomerById_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerService.Setup(s => s.GetByIdAsync(customerId)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _controller.GetCustomerById(customerId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockCustomerService.Verify(s => s.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnCreatedWithCustomer_WhenValidRequest()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "John Doe",
            Email = "john@email.com",
            Phone = "123-456-7890"
        };
        var createdCustomer = new Customer(request.Name, request.Email, request.Phone);
        _mockCustomerService.Setup(s => s.CreateAsync(request.Name, request.Email, request.Phone))
                           .ReturnsAsync(createdCustomer);

        // Act
        var result = await _controller.CreateCustomer(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().BeOfType<CustomerResponse>();
        var response = createdResult.Value as CustomerResponse;
        response!.Name.Should().Be(request.Name);
        response.Email.Should().Be(request.Email);
        response.Phone.Should().Be(request.Phone);
        
        _mockCustomerService.Verify(s => s.CreateAsync(request.Name, request.Email, request.Phone), Times.Once);
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "",
            Email = "john@email.com",
            Phone = "123-456-7890"
        };
        _mockCustomerService.Setup(s => s.CreateAsync(request.Name, request.Email, request.Phone))
                           .ThrowsAsync(new ArgumentException("Name cannot be null or empty"));

        // Act
        var result = await _controller.CreateCustomer(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeOfType<string>();
        badRequestResult.Value.Should().Be("Name cannot be null or empty");
        
        _mockCustomerService.Verify(s => s.CreateAsync(request.Name, request.Email, request.Phone), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnOkWithUpdatedCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = 1;
        var request = new UpdateCustomerRequest
        {
            Name = "Jane Smith",
            Email = "jane@email.com",
            Phone = "987-654-3210"
        };
        var updatedCustomer = new Customer(request.Name, request.Email, request.Phone);
        _mockCustomerService.Setup(s => s.UpdateAsync(customerId, request.Name, request.Email, request.Phone))
                           .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.UpdateCustomer(customerId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<CustomerResponse>();
        var response = okResult.Value as CustomerResponse;
        response!.Name.Should().Be(request.Name);
        response.Email.Should().Be(request.Email);
        response.Phone.Should().Be(request.Phone);
        
        _mockCustomerService.Verify(s => s.UpdateAsync(customerId, request.Name, request.Email, request.Phone), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 999;
        var request = new UpdateCustomerRequest
        {
            Name = "Jane Smith",
            Email = "jane@email.com",
            Phone = "987-654-3210"
        };
        _mockCustomerService.Setup(s => s.UpdateAsync(customerId, request.Name, request.Email, request.Phone))
                           .ReturnsAsync((Customer?)null);

        // Act
        var result = await _controller.UpdateCustomer(customerId, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockCustomerService.Verify(s => s.UpdateAsync(customerId, request.Name, request.Email, request.Phone), Times.Once);
    }

    [Fact]
    public async Task UpdateCustomer_ShouldReturnBadRequest_WhenServiceThrowsArgumentException()
    {
        // Arrange
        var customerId = 1;
        var request = new UpdateCustomerRequest
        {
            Name = "",
            Email = "jane@email.com",
            Phone = "987-654-3210"
        };
        _mockCustomerService.Setup(s => s.UpdateAsync(customerId, request.Name, request.Email, request.Phone))
                           .ThrowsAsync(new ArgumentException("Name cannot be null or empty"));

        // Act
        var result = await _controller.UpdateCustomer(customerId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().BeOfType<string>();
        badRequestResult.Value.Should().Be("Name cannot be null or empty");
        
        _mockCustomerService.Verify(s => s.UpdateAsync(customerId, request.Name, request.Email, request.Phone), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomer_ShouldReturnNoContent_WhenCustomerExists()
    {
        // Arrange
        var customerId = 1;
        _mockCustomerService.Setup(s => s.DeleteAsync(customerId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockCustomerService.Verify(s => s.DeleteAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task DeleteCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerService.Setup(s => s.DeleteAsync(customerId)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCustomer(customerId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _mockCustomerService.Verify(s => s.DeleteAsync(customerId), Times.Once);
    }
}
