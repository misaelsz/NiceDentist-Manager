using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Tests.Domain;

/// <summary>
/// Unit tests for Customer domain entity
/// </summary>
public class CustomerTests
{
    [Fact]
    public void Customer_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.Id.Should().Be(0);
        customer.Name.Should().BeEmpty();
        customer.Email.Should().BeEmpty();
        customer.Phone.Should().BeEmpty();
        customer.Address.Should().BeEmpty();
        customer.DateOfBirth.Should().Be(default);
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        customer.IsActive.Should().BeTrue();
        customer.Appointments.Should().NotBeNull();
        customer.Appointments.Should().BeEmpty();
    }

    [Fact]
    public void Customer_SetProperties_ShouldSetCorrectly()
    {
        // Arrange
        var customer = new Customer();
        var birthDate = new DateTime(1990, 5, 15);
        var createdAt = DateTime.UtcNow.AddDays(-10);
        var updatedAt = DateTime.UtcNow.AddDays(-1);

        // Act
        customer.Id = 1;
        customer.Name = "John Doe";
        customer.Email = "john.doe@email.com";
        customer.Phone = "+1234567890";
        customer.Address = "123 Main St, City, State";
        customer.DateOfBirth = birthDate;
        customer.CreatedAt = createdAt;
        customer.UpdatedAt = updatedAt;
        customer.IsActive = false;

        // Assert
        customer.Id.Should().Be(1);
        customer.Name.Should().Be("John Doe");
        customer.Email.Should().Be("john.doe@email.com");
        customer.Phone.Should().Be("+1234567890");
        customer.Address.Should().Be("123 Main St, City, State");
        customer.DateOfBirth.Should().Be(birthDate);
        customer.CreatedAt.Should().Be(createdAt);
        customer.UpdatedAt.Should().Be(updatedAt);
        customer.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Customer_EmptyOrNullName_ShouldBeHandled(string? name)
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Name = name ?? string.Empty;

        // Assert
        customer.Name.Should().Be(name ?? string.Empty);
    }

    [Theory]
    [InlineData("test@email.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("simple@domain.org")]
    public void Customer_ValidEmail_ShouldBeAccepted(string email)
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Email = email;

        // Assert
        customer.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("(555) 123-4567")]
    [InlineData("555-123-4567")]
    [InlineData("55512344567")]
    public void Customer_PhoneFormats_ShouldBeAccepted(string phone)
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Phone = phone;

        // Assert
        customer.Phone.Should().Be(phone);
    }

    [Fact]
    public void Customer_AppointmentsCollection_ShouldBeModifiable()
    {
        // Arrange
        var customer = new Customer();
        var appointment = new Appointment();

        // Act
        customer.Appointments.Add(appointment);

        // Assert
        customer.Appointments.Should().HaveCount(1);
        customer.Appointments.Should().Contain(appointment);
    }

    [Fact]
    public void Customer_DateOfBirth_ShouldAcceptValidDates()
    {
        // Arrange
        var customer = new Customer();
        var validBirthDate = new DateTime(1985, 3, 15);

        // Act
        customer.DateOfBirth = validBirthDate;

        // Assert
        customer.DateOfBirth.Should().Be(validBirthDate);
    }

    [Fact]
    public void Customer_TimestampProperties_ShouldBeSettable()
    {
        // Arrange
        var customer = new Customer();
        var specificTime = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        customer.CreatedAt = specificTime;
        customer.UpdatedAt = specificTime.AddHours(1);

        // Assert
        customer.CreatedAt.Should().Be(specificTime);
        customer.UpdatedAt.Should().Be(specificTime.AddHours(1));
    }

    [Fact]
    public void Customer_IsActive_ShouldBeToggleable()
    {
        // Arrange
        var customer = new Customer();

        // Act & Assert - Initial state
        customer.IsActive.Should().BeTrue();

        // Act - Deactivate
        customer.IsActive = false;

        // Assert
        customer.IsActive.Should().BeFalse();

        // Act - Reactivate
        customer.IsActive = true;

        // Assert
        customer.IsActive.Should().BeTrue();
    }
}
