using FluentAssertions;
using Moq;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.Services;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Application;

/// <summary>
/// Unit tests for CustomerService (Working Version)
/// </summary>
public class CustomerServiceWorkingTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IAuthApiService> _mockAuthApiService;
    private readonly CustomerService _service;

    public CustomerServiceWorkingTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockAuthApiService = new Mock<IAuthApiService>();
        _service = new CustomerService(_mockCustomerRepository.Object, _mockAuthApiService.Object);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnCustomer()
    {
        // Arrange
        var email = "test@example.com";
        var customer = new Customer { Id = 1, Email = email, FullName = "Test User" };
        _mockCustomerRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(customer);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        _mockCustomerRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer { Id = 1, Email = "test1@example.com", FullName = "Test User 1" },
            new Customer { Id = 2, Email = "test2@example.com", FullName = "Test User 2" }
        };
        _mockCustomerRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(customers);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        _mockCustomerRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer { Id = customerId, Email = "test@example.com", FullName = "Test User" };
        _mockCustomerRepository.Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _service.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        _mockCustomerRepository.Verify(x => x.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidCustomer_ShouldCreateSuccessfully()
    {
        // Arrange
        var customer = new Customer { Email = "test@example.com", FullName = "Test User" };
        _mockCustomerRepository.Setup(x => x.CreateAsync(customer))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(customer);

        // Assert
        result.Should().Be(1);
        _mockCustomerRepository.Verify(x => x.CreateAsync(customer), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var customerId = 1;
        _mockCustomerRepository.Setup(x => x.DeleteAsync(customerId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(customerId);

        // Assert
        _mockCustomerRepository.Verify(x => x.DeleteAsync(customerId), Times.Once);
    }
}
