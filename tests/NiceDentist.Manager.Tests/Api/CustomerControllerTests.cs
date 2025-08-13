/*
using Microsoft.AspNetCore.Mvc;
using NiceDentist.Manager.Api.Controllers;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Tests.Api;

/// <summary>
/// Unit tests for CustomerController
/// </summary>
public class CustomerControllerTests
{
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly CustomerController _controller;

    public CustomerControllerTests()
    {
        _customerServiceMock = new Mock<ICustomerService>();
        _controller = new CustomerController(_customerServiceMock.Object);
    }

    [Fact]
    public async Task GetAll_ValidRequest_ShouldReturnOkWithCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@email.com" },
            new() { Id = 2, Name = "Jane Smith", Email = "jane@email.com" }
        };

        _customerServiceMock.Setup(x => x.GetAllCustomersAsync(1, 10))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetAll(1, 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCustomers = okResult.Value.Should().BeAssignableTo<IEnumerable<Customer>>().Subject;
        returnedCustomers.Should().BeEquivalentTo(customers);

        _customerServiceMock.Verify(x => x.GetAllCustomersAsync(1, 10), Times.Once);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task GetAll_InvalidPagination_ShouldReturnBadRequest(int page, int pageSize)
    {
        // Act
        var result = await _controller.GetAll(page, pageSize);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _customerServiceMock.Verify(x => x.GetAllCustomersAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetById_ExistingId_ShouldReturnOkWithCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer { Id = customerId, Name = "John Doe", Email = "john@email.com" };

        _customerServiceMock.Setup(x => x.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.GetById(customerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCustomer = okResult.Value.Should().BeAssignableTo<Customer>().Subject;
        returnedCustomer.Should().BeEquivalentTo(customer);

        _customerServiceMock.Verify(x => x.GetCustomerByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task GetById_NonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = 999;

        _customerServiceMock.Setup(x => x.GetCustomerByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _controller.GetById(customerId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        _customerServiceMock.Verify(x => x.GetCustomerByIdAsync(customerId), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetById_InvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Act
        var result = await _controller.GetById(invalidId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _customerServiceMock.Verify(x => x.GetCustomerByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Create_ValidCustomer_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "John Doe",
            Email = "john@email.com",
            Phone = "+1234567890",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var createdCustomer = new Customer
        {
            Id = 1,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            DateOfBirth = customer.DateOfBirth,
            CreatedAt = DateTime.UtcNow
        };

        var serviceResult = new CustomerServiceResult
        {
            Success = true,
            Message = "Customer created successfully.",
            Customer = createdCustomer
        };

        _customerServiceMock.Setup(x => x.CreateCustomerAsync(customer))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Create(customer);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CustomerController.GetById));
        createdResult.RouteValues!["id"].Should().Be(createdCustomer.Id);
        
        var returnedCustomer = createdResult.Value.Should().BeAssignableTo<Customer>().Subject;
        returnedCustomer.Should().BeEquivalentTo(createdCustomer);

        _customerServiceMock.Verify(x => x.CreateCustomerAsync(customer), Times.Once);
    }

    [Fact]
    public async Task Create_InvalidCustomer_ShouldReturnBadRequest()
    {
        // Arrange
        var customer = new Customer(); // Invalid customer with empty properties

        var serviceResult = new CustomerServiceResult
        {
            Success = false,
            Message = "Name, email and phone are required.",
            Customer = null
        };

        _customerServiceMock.Setup(x => x.CreateCustomerAsync(customer))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Create(customer);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be(serviceResult.Message);

        _customerServiceMock.Verify(x => x.CreateCustomerAsync(customer), Times.Once);
    }

    [Fact]
    public async Task Create_NullCustomer_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _customerServiceMock.Verify(x => x.CreateCustomerAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task Update_ValidCustomer_ShouldReturnOk()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer
        {
            Id = customerId,
            Name = "John Doe Updated",
            Email = "john.updated@email.com",
            Phone = "+1234567890"
        };

        var serviceResult = new CustomerServiceResult
        {
            Success = true,
            Message = "Customer updated successfully."
        };

        _customerServiceMock.Setup(x => x.UpdateCustomerAsync(customer))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Update(customerId, customer);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(serviceResult.Message);

        _customerServiceMock.Verify(x => x.UpdateCustomerAsync(customer), Times.Once);
    }

    [Fact]
    public async Task Update_MismatchedIds_ShouldReturnBadRequest()
    {
        // Arrange
        var routeId = 1;
        var customer = new Customer { Id = 2, Name = "John Doe" };

        // Act
        var result = await _controller.Update(routeId, customer);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _customerServiceMock.Verify(x => x.UpdateCustomerAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task Update_InvalidCustomer_ShouldReturnBadRequest()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer { Id = customerId }; // Invalid customer

        var serviceResult = new CustomerServiceResult
        {
            Success = false,
            Message = "Invalid customer ID."
        };

        _customerServiceMock.Setup(x => x.UpdateCustomerAsync(customer))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Update(customerId, customer);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.Value.Should().Be(serviceResult.Message);
    }

    [Fact]
    public async Task Delete_ExistingCustomer_ShouldReturnOk()
    {
        // Arrange
        var customerId = 1;

        var serviceResult = new CustomerServiceResult
        {
            Success = true,
            Message = "Customer deleted successfully."
        };

        _customerServiceMock.Setup(x => x.DeleteCustomerAsync(customerId))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Delete(customerId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(serviceResult.Message);

        _customerServiceMock.Verify(x => x.DeleteCustomerAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task Delete_NonExistingCustomer_ShouldReturnNotFound()
    {
        // Arrange
        var customerId = 999;

        var serviceResult = new CustomerServiceResult
        {
            Success = false,
            Message = "Customer not found."
        };

        _customerServiceMock.Setup(x => x.DeleteCustomerAsync(customerId))
            .ReturnsAsync(serviceResult);

        // Act
        var result = await _controller.Delete(customerId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().Be(serviceResult.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Delete_InvalidId_ShouldReturnBadRequest(int invalidId)
    {
        // Act
        var result = await _controller.Delete(invalidId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _customerServiceMock.Verify(x => x.DeleteCustomerAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Search_ValidKeyword_ShouldReturnOkWithResults()
    {
        // Arrange
        var keyword = "John";
        var customers = new List<Customer>
        {
            new() { Id = 1, Name = "John Doe", Email = "john@email.com" },
            new() { Id = 2, Name = "Johnny Smith", Email = "johnny@email.com" }
        };

        _customerServiceMock.Setup(x => x.SearchCustomersAsync(keyword))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.Search(keyword);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCustomers = okResult.Value.Should().BeAssignableTo<IEnumerable<Customer>>().Subject;
        returnedCustomers.Should().BeEquivalentTo(customers);

        _customerServiceMock.Verify(x => x.SearchCustomersAsync(keyword), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Search_InvalidKeyword_ShouldReturnBadRequest(string? invalidKeyword)
    {
        // Act
        var result = await _controller.Search(invalidKeyword!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _customerServiceMock.Verify(x => x.SearchCustomersAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByEmail_ValidEmail_ShouldReturnOkWithCustomer()
    {
        // Arrange
        var email = "john@email.com";
        var customer = new Customer { Id = 1, Name = "John Doe", Email = email };

        _customerServiceMock.Setup(x => x.GetCustomerByEmailAsync(email))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.GetByEmail(email);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedCustomer = okResult.Value.Should().BeAssignableTo<Customer>().Subject;
        returnedCustomer.Should().BeEquivalentTo(customer);
    }

    [Fact]
    public async Task GetByEmail_NonExistingEmail_ShouldReturnNotFound()
    {
        // Arrange
        var email = "nonexistent@email.com";

        _customerServiceMock.Setup(x => x.GetCustomerByEmailAsync(email))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _controller.GetByEmail(email);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    public async Task GetByEmail_InvalidEmail_ShouldReturnBadRequest(string? invalidEmail)
    {
        // Act
        var result = await _controller.GetByEmail(invalidEmail!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _customerServiceMock.Verify(x => x.GetCustomerByEmailAsync(It.IsAny<string>()), Times.Never);
    }
}
*/
