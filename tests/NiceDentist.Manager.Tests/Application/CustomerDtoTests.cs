using FluentAssertions;
using NiceDentist.Manager.Application.DTOs;
using Xunit;

namespace NiceDentist.Manager.Tests.Application;

/// <summary>
/// Unit tests for CustomerDto
/// </summary>
public class CustomerDtoTests
{
    [Fact]
    public void CustomerDto_ShouldCreateWithDefaultValues()
    {
        // Act
        var customerDto = new CustomerDto();

        // Assert
        customerDto.Should().NotBeNull();
        customerDto.Id.Should().Be(0);
        customerDto.Name.Should().Be(string.Empty);
        customerDto.Email.Should().Be(string.Empty);
        customerDto.Phone.Should().BeNull();
        customerDto.DateOfBirth.Should().BeNull();
        customerDto.Address.Should().BeNull();
        customerDto.CreatedAt.Should().Be(default(DateTime));
        customerDto.UpdatedAt.Should().Be(default(DateTime));
        customerDto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CustomerDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var customerDto = new CustomerDto();
        var id = 1;
        var name = "John Doe";
        var email = "john.doe@email.com";
        var phone = "123-456-7890";
        var dateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var address = "123 Main St";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddHours(1);
        var isActive = true;

        // Act
        customerDto.Id = id;
        customerDto.Name = name;
        customerDto.Email = email;
        customerDto.Phone = phone;
        customerDto.DateOfBirth = dateOfBirth;
        customerDto.Address = address;
        customerDto.CreatedAt = createdAt;
        customerDto.UpdatedAt = updatedAt;
        customerDto.IsActive = isActive;

        // Assert
        customerDto.Id.Should().Be(id);
        customerDto.Name.Should().Be(name);
        customerDto.Email.Should().Be(email);
        customerDto.Phone.Should().Be(phone);
        customerDto.DateOfBirth.Should().Be(dateOfBirth);
        customerDto.Address.Should().Be(address);
        customerDto.CreatedAt.Should().Be(createdAt);
        customerDto.UpdatedAt.Should().Be(updatedAt);
        customerDto.IsActive.Should().Be(isActive);
    }

    [Fact]
    public void CustomerDto_ShouldHandleNullableProperties()
    {
        // Arrange
        var customerDto = new CustomerDto
        {
            Name = "John Doe",
            Email = "john@email.com",
            Phone = null,
            DateOfBirth = null,
            Address = null
        };

        // Assert
        customerDto.Phone.Should().BeNull();
        customerDto.DateOfBirth.Should().BeNull();
        customerDto.Address.Should().BeNull();
        customerDto.Name.Should().Be("John Doe");
        customerDto.Email.Should().Be("john@email.com");
    }

    [Fact]
    public void CustomerDto_ShouldAllowObjectInitializerSyntax()
    {
        // Arrange & Act
        var customerDto = new CustomerDto
        {
            Id = 1,
            Name = "Jane Smith",
            Email = "jane@email.com",
            Phone = "987-654-3210",
            DateOfBirth = new DateTime(1985, 5, 15, 0, 0, 0, DateTimeKind.Utc),
            Address = "456 Oak Avenue",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        customerDto.Id.Should().Be(1);
        customerDto.Name.Should().Be("Jane Smith");
        customerDto.Email.Should().Be("jane@email.com");
        customerDto.Phone.Should().Be("987-654-3210");
        customerDto.DateOfBirth.Should().Be(new DateTime(1985, 5, 15, 0, 0, 0, DateTimeKind.Utc));
        customerDto.Address.Should().Be("456 Oak Avenue");
        customerDto.IsActive.Should().BeTrue();
    }
}
