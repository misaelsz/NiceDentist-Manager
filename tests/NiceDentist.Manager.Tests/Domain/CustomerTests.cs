using FluentAssertions;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Domain;

/// <summary>
/// Unit tests for Customer entity
/// </summary>
public class CustomerTests
{
    [Fact]
    public void Customer_ShouldCreateWithDefaultValues()
    {
        // Act
        var customer = new Customer();

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().Be(0);
        customer.Name.Should().Be(string.Empty);
        customer.Email.Should().Be(string.Empty);
        customer.Phone.Should().Be(string.Empty);
        customer.Address.Should().Be(string.Empty);
        customer.IsActive.Should().BeTrue();
        customer.DateOfBirth.Should().Be(default(DateTime));
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        customer.Appointments.Should().NotBeNull();
        customer.Appointments.Should().BeEmpty();
    }

    [Fact]
    public void Customer_ShouldAllowSettingProperties()
    {
        // Arrange
        var customer = new Customer();
        var name = "John Doe";
        var email = "john.doe@email.com";
        var phone = "123-456-7890";
        var address = "123 Main St";
        var dateOfBirth = new DateTime(1990, 1, 1);

        // Act
        customer.Name = name;
        customer.Email = email;
        customer.Phone = phone;
        customer.Address = address;
        customer.DateOfBirth = dateOfBirth;
        customer.IsActive = false;

        // Assert
        customer.Name.Should().Be(name);
        customer.Email.Should().Be(email);
        customer.Phone.Should().Be(phone);
        customer.Address.Should().Be(address);
        customer.DateOfBirth.Should().Be(dateOfBirth);
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Customer_ShouldAllowUpdatingTimestamps()
    {
        // Arrange
        var customer = new Customer();
        var newUpdatedTime = DateTime.UtcNow.AddHours(1);

        // Act
        customer.UpdatedAt = newUpdatedTime;

        // Assert
        customer.UpdatedAt.Should().Be(newUpdatedTime);
    }
}
