using FluentAssertions;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Domain;

/// <summary>
/// Unit tests for Customer business rules and validations
/// </summary>
public class CustomerValidationTests
{
    [Fact]
    public void Customer_ShouldHaveValidDefaultTimestamps()
    {
        // Act
        var customer = new Customer();

        // Assert
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        customer.CreatedAt.Should().BeCloseTo(customer.UpdatedAt, TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void Customer_ShouldBeActiveByDefault()
    {
        // Act
        var customer = new Customer();

        // Assert
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Customer_ShouldHaveEmptyCollectionOfAppointments()
    {
        // Act
        var customer = new Customer();

        // Assert
        customer.Appointments.Should().NotBeNull();
        customer.Appointments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Customer_ShouldAcceptEmptyStringsForOptionalFields(string emptyValue)
    {
        // Act
        var customer = new Customer
        {
            Name = emptyValue,
            Email = emptyValue,
            Phone = emptyValue,
            Address = emptyValue
        };

        // Assert - Should not throw exceptions
        customer.Name.Should().Be(emptyValue);
        customer.Email.Should().Be(emptyValue);
        customer.Phone.Should().Be(emptyValue);
        customer.Address.Should().Be(emptyValue);
    }

    [Fact]
    public void Customer_ShouldAllowDeactivation()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.IsActive = false;

        // Assert
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Customer_ShouldAllowManualTimestampUpdates()
    {
        // Arrange
        var customer = new Customer();
        var newTime = DateTime.UtcNow.AddDays(1);

        // Act
        customer.UpdatedAt = newTime;

        // Assert
        customer.UpdatedAt.Should().Be(newTime);
        customer.CreatedAt.Should().NotBe(newTime);
    }

    [Fact]
    public void Customer_ShouldAllowDateOfBirthSetting()
    {
        // Arrange
        var customer = new Customer();
        var dateOfBirth = new DateTime(1990, 5, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        customer.DateOfBirth = dateOfBirth;

        // Assert
        customer.DateOfBirth.Should().Be(dateOfBirth);
    }
}
