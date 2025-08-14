using FluentAssertions;
using NiceDentist.Manager.Domain;
using NiceDentist.Manager.Infrastructure.Repositories;
using Xunit;

namespace NiceDentist.Manager.Tests.Infrastructure;

/// <summary>
/// Unit tests for InMemoryCustomerRepository
/// </summary>
public class InMemoryCustomerRepositoryTests
{
    private readonly InMemoryCustomerRepository _repository;

    public InMemoryCustomerRepositoryTests()
    {
        _repository = new InMemoryCustomerRepository();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCustomersExist()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldAddCustomerAndReturnWithId()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "John Doe",
            Email = "john@email.com",
            Phone = "123-456-7890"
        };

        // Act
        var result = await _repository.CreateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@email.com");
        result.Phone.Should().Be("123-456-7890");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCustomers_AfterCreating()
    {
        // Arrange
        var customer1 = new Customer { Name = "John Doe", Email = "john@email.com", Phone = "123-456-7890" };
        var customer2 = new Customer { Name = "Jane Smith", Email = "jane@email.com", Phone = "987-654-3210" };
        
        await _repository.CreateAsync(customer1);
        await _repository.CreateAsync(customer2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "John Doe");
        result.Should().Contain(c => c.Name == "Jane Smith");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customer = new Customer { Name = "John Doe", Email = "john@email.com", Phone = "123-456-7890" };
        var createdCustomer = await _repository.CreateAsync(customer);

        // Act
        var result = await _repository.GetByIdAsync(createdCustomer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdCustomer.Id);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@email.com");
        result.Phone.Should().Be("123-456-7890");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenCustomerExists()
    {
        // Arrange
        var customer = new Customer { Name = "John Doe", Email = "john@email.com", Phone = "123-456-7890" };
        var createdCustomer = await _repository.CreateAsync(customer);

        // Act
        var result = await _repository.DeleteAsync(createdCustomer.Id);

        // Assert
        result.Should().BeTrue();

        // Verify customer is removed
        var deletedCustomer = await _repository.GetByIdAsync(createdCustomer.Id);
        deletedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenCustomerDoesNotExist()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }
}
