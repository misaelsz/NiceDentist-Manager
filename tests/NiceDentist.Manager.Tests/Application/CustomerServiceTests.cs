using FluentAssertions;
using Moq;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.Services;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Application;

/// <summary>
/// Unit tests for CustomerService
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IAuthApiService> _mockAuthApiService;
    private readonly CustomerService _customerService;

    public CustomerServiceTests()
    {
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockAuthApiService = new Mock<IAuthApiService>();
        _customerService = new CustomerService(_mockCustomerRepository.Object, _mockAuthApiService.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer("John Doe", "john@email.com", "123-456-7890"),
            new Customer("Jane Smith", "jane@email.com", "987-654-3210")
        };
        _mockCustomerRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

        // Act
        var result = await _customerService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(customers);
        _mockCustomerRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer("John Doe", "john@email.com", "123-456-7890");
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(customer);
        _mockCustomerRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
        _mockCustomerRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnCustomer_WhenValidDataProvided()
    {
        // Arrange
        var name = "John Doe";
        var email = "john@email.com";
        var phone = "123-456-7890";
        var createdCustomer = new Customer(name, email, phone);
        
        _mockCustomerRepository.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
                              .ReturnsAsync(createdCustomer);

        // Act
        var result = await _customerService.CreateAsync(name, email, phone);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Email.Should().Be(email);
        result.Phone.Should().Be(phone);
        _mockCustomerRepository.Verify(r => r.CreateAsync(It.Is<Customer>(c => 
            c.Name == name && c.Email == email && c.Phone == phone)), Times.Once);
    }

    [Theory]
    [InlineData("", "john@email.com", "123-456-7890")]
    [InlineData("John Doe", "", "123-456-7890")]
    [InlineData("John Doe", "john@email.com", "")]
    [InlineData(null, "john@email.com", "123-456-7890")]
    [InlineData("John Doe", null, "123-456-7890")]
    [InlineData("John Doe", "john@email.com", null)]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenInvalidDataProvided(string name, string email, string phone)
    {
        // Act & Assert
        var act = async () => await _customerService.CreateAsync(name, email, phone);
        await act.Should().ThrowAsync<ArgumentException>();
        
        _mockCustomerRepository.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = 1;
        var existingCustomer = new Customer("John Doe", "john@email.com", "123-456-7890");
        var updatedName = "Jane Smith";
        var updatedEmail = "jane@email.com";
        var updatedPhone = "987-654-3210";

        _mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(existingCustomer);
        _mockCustomerRepository.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
                              .ReturnsAsync((Customer c) => c);

        // Act
        var result = await _customerService.UpdateAsync(customerId, updatedName, updatedEmail, updatedPhone);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updatedName);
        result.Email.Should().Be(updatedEmail);
        result.Phone.Should().Be(updatedPhone);
        _mockCustomerRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _mockCustomerRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.UpdateAsync(customerId, "Updated Name", "updated@email.com", "555-0000");

        // Assert
        result.Should().BeNull();
        _mockCustomerRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _mockCustomerRepository.Verify(r => r.UpdateAsync(It.IsAny<Customer>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenCustomerExists()
    {
        // Arrange
        var customerId = 1;
        var existingCustomer = new Customer("John Doe", "john@email.com", "123-456-7890");
        
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync(existingCustomer);
        _mockCustomerRepository.Setup(r => r.DeleteAsync(customerId)).ReturnsAsync(true);

        // Act
        var result = await _customerService.DeleteAsync(customerId);

        // Assert
        result.Should().BeTrue();
        _mockCustomerRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _mockCustomerRepository.Verify(r => r.DeleteAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(customerId)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.DeleteAsync(customerId);

        // Assert
        result.Should().BeFalse();
        _mockCustomerRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
        _mockCustomerRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var email = "john@email.com";
        var customer = new Customer("John Doe", email, "123-456-7890");
        _mockCustomerRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(customer);

        // Act
        var result = await _customerService.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(customer);
        _mockCustomerRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@email.com";
        _mockCustomerRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((Customer?)null);

        // Act
        var result = await _customerService.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
        _mockCustomerRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }
}
