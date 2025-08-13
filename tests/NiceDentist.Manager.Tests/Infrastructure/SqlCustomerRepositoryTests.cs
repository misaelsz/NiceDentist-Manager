/*
using NiceDentist.Manager.Domain;
using NiceDentist.Manager.Infrastructure;
using Microsoft.Data.SqlClient;
using System.Data;

namespace NiceDentist.Manager.Tests.Infrastructure;

/// <summary>
/// Unit tests for SqlCustomerRepository
/// </summary>
public class SqlCustomerRepositoryTests
{
    private readonly Mock<IDbConnection> _connectionMock;
    private readonly Mock<IDbCommand> _commandMock;
    private readonly Mock<IDataReader> _readerMock;
    private readonly SqlCustomerRepository _repository;

    public SqlCustomerRepositoryTests()
    {
        _connectionMock = new Mock<IDbConnection>();
        _commandMock = new Mock<IDbCommand>();
        _readerMock = new Mock<IDataReader>();
        
        _connectionMock.Setup(x => x.CreateCommand()).Returns(_commandMock.Object);
        _commandMock.Setup(x => x.ExecuteReader()).Returns(_readerMock.Object);
        
        _repository = new SqlCustomerRepository("Server=test;Database=test;");
    }

    [Fact]
    public async Task CreateAsync_ValidCustomer_ShouldReturnCustomerWithId()
    {
        // Arrange
        var customer = new Customer
        {
            Name = "John Doe",
            Email = "john@email.com",
            Phone = "+1234567890",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Main St"
        };

        var expectedId = 1;
        _commandMock.Setup(x => x.ExecuteScalar()).Returns(expectedId);

        // Act
        var result = await _repository.CreateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(expectedId);
        result.Name.Should().Be(customer.Name);
        result.Email.Should().Be(customer.Email);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _commandMock.Verify(x => x.ExecuteScalar(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = 1;
        SetupDataReader(new[]
        {
            new Customer
            {
                Id = customerId,
                Name = "John Doe",
                Email = "john@email.com",
                Phone = "+1234567890",
                DateOfBirth = new DateTime(1990, 1, 1),
                Address = "123 Main St",
                CreatedAt = DateTime.UtcNow
            }
        });

        // Act
        var result = await _repository.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.Name.Should().Be("John Doe");
        result.Email.Should().Be("john@email.com");

        _commandMock.Verify(x => x.ExecuteReader(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ShouldReturnNull()
    {
        // Arrange
        var customerId = 999;
        SetupDataReader(Array.Empty<Customer>());

        // Act
        var result = await _repository.GetByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
        _commandMock.Verify(x => x.ExecuteReader(), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ShouldReturnCustomer()
    {
        // Arrange
        var email = "john@email.com";
        var expectedCustomer = new Customer
        {
            Id = 1,
            Name = "John Doe",
            Email = email,
            Phone = "+1234567890",
            CreatedAt = DateTime.UtcNow
        };

        SetupDataReader(new[] { expectedCustomer });

        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ShouldReturnCustomers()
    {
        // Arrange
        var customers = new[]
        {
            new Customer { Id = 1, Name = "John Doe", Email = "john@email.com" },
            new Customer { Id = 2, Name = "Jane Smith", Email = "jane@email.com" }
        };

        SetupDataReader(customers);

        // Act
        var result = await _repository.GetAllAsync(1, 10);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(customers);
    }

    [Fact]
    public async Task UpdateAsync_ValidCustomer_ShouldExecuteCommand()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Name = "John Doe Updated",
            Email = "john.updated@email.com",
            Phone = "+1234567890",
            UpdatedAt = DateTime.UtcNow
        };

        _commandMock.Setup(x => x.ExecuteNonQuery()).Returns(1);

        // Act
        await _repository.UpdateAsync(customer);

        // Assert
        _commandMock.Verify(x => x.ExecuteNonQuery(), Times.Once);
        _commandMock.VerifySet(x => x.CommandText = It.IsAny<string>(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ShouldReturnTrue()
    {
        // Arrange
        var customerId = 1;
        _commandMock.Setup(x => x.ExecuteNonQuery()).Returns(1);

        // Act
        var result = await _repository.DeleteAsync(customerId);

        // Assert
        result.Should().BeTrue();
        _commandMock.Verify(x => x.ExecuteNonQuery(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var customerId = 999;
        _commandMock.Setup(x => x.ExecuteNonQuery()).Returns(0);

        // Act
        var result = await _repository.DeleteAsync(customerId);

        // Assert
        result.Should().BeFalse();
        _commandMock.Verify(x => x.ExecuteNonQuery(), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_WithKeyword_ShouldReturnMatchingCustomers()
    {
        // Arrange
        var keyword = "John";
        var matchingCustomers = new[]
        {
            new Customer { Id = 1, Name = "John Doe", Email = "john@email.com" },
            new Customer { Id = 3, Name = "Johnny Smith", Email = "johnny@email.com" }
        };

        SetupDataReader(matchingCustomers);

        // Act
        var result = await _repository.SearchAsync(keyword);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetCustomersByDateRangeAsync_ValidRange_ShouldReturnCustomers()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var customers = new[]
        {
            new Customer 
            { 
                Id = 1, 
                Name = "John Doe", 
                Email = "john@email.com",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        SetupDataReader(customers);

        // Act
        var result = await _repository.GetCustomersByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result.First().CreatedAt.Should().BeAfter(startDate).And.BeBefore(endDate);
    }

    private void SetupDataReader(Customer[] customers)
    {
        var readCalls = 0;
        _readerMock.Setup(x => x.Read())
            .Returns(() => readCalls < customers.Length)
            .Callback(() => readCalls++);

        _readerMock.Setup(x => x["Id"])
            .Returns(() => readCalls <= customers.Length ? customers[readCalls - 1].Id : DBNull.Value);
        _readerMock.Setup(x => x["Name"])
            .Returns(() => readCalls <= customers.Length ? customers[readCalls - 1].Name : DBNull.Value);
        _readerMock.Setup(x => x["Email"])
            .Returns(() => readCalls <= customers.Length ? customers[readCalls - 1].Email : DBNull.Value);
        _readerMock.Setup(x => x["Phone"])
            .Returns(() => readCalls <= customers.Length ? customers[readCalls - 1].Phone ?? DBNull.Value : DBNull.Value);
        _readerMock.Setup(x => x["DateOfBirth"])
            .Returns(() => readCalls <= customers.Length ? customers[readCalls - 1].DateOfBirth : DBNull.Value);
        _readerMock.Setup(x => x["Address"])
            .Returns(() => readCalls <= customers.Length ? customers[readCalls - 1].Address ?? DBNull.Value : DBNull.Value);
        _readerMock.Setup(x => x["CreatedAt"])
            .Returns(() => readCalls <= customers.Length ? customers[readCalls - 1].CreatedAt : DBNull.Value);
        _readerMock.Setup(x => x["UpdatedAt"])
            .Returns(() => readCalls <= customers.Length ? customers[readCalls - 1].UpdatedAt : DBNull.Value);
    }
}
*/
