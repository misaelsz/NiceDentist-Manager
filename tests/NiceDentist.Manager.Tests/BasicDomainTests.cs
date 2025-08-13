using FluentAssertions;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Simple;

/// <summary>
/// Basic working tests for domain entities
/// </summary>
public class BasicDomainTests
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
        customer.IsActive.Should().BeTrue();
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        customer.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Customer_SetProperties_ShouldSetCorrectly()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Id = 1;
        customer.Name = "John Doe";
        customer.Email = "john@example.com";
        customer.Phone = "123-456-7890";
        customer.Address = "123 Main St";
        customer.IsActive = false;

        // Assert
        customer.Id.Should().Be(1);
        customer.Name.Should().Be("John Doe");
        customer.Email.Should().Be("john@example.com");
        customer.Phone.Should().Be("123-456-7890");
        customer.Address.Should().Be("123 Main St");
        customer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Dentist_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var dentist = new Dentist();

        // Assert
        dentist.Id.Should().Be(0);
        dentist.Name.Should().BeEmpty();
        dentist.Email.Should().BeEmpty();
        dentist.Phone.Should().BeEmpty();
        dentist.LicenseNumber.Should().BeEmpty();
        dentist.Specialization.Should().BeEmpty();
        dentist.IsActive.Should().BeTrue();
        dentist.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dentist.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Dentist_SetProperties_ShouldSetCorrectly()
    {
        // Arrange
        var dentist = new Dentist();

        // Act
        dentist.Id = 1;
        dentist.Name = "Dr. Smith";
        dentist.Email = "dr.smith@example.com";
        dentist.Phone = "123-456-7890";
        dentist.LicenseNumber = "D12345";
        dentist.Specialization = "Orthodontics";
        dentist.IsActive = false;

        // Assert
        dentist.Id.Should().Be(1);
        dentist.Name.Should().Be("Dr. Smith");
        dentist.Email.Should().Be("dr.smith@example.com");
        dentist.Phone.Should().Be("123-456-7890");
        dentist.LicenseNumber.Should().Be("D12345");
        dentist.Specialization.Should().Be("Orthodontics");
        dentist.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Appointment_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var appointment = new Appointment();

        // Assert
        appointment.Id.Should().Be(0);
        appointment.CustomerId.Should().Be(0);
        appointment.DentistId.Should().Be(0);
        appointment.ProcedureType.Should().BeEmpty();
        appointment.Notes.Should().BeEmpty();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        appointment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Appointment_SetProperties_ShouldSetCorrectly()
    {
        // Arrange
        var appointment = new Appointment();
        var appointmentDate = DateTime.UtcNow.AddDays(1);

        // Act
        appointment.Id = 1;
        appointment.CustomerId = 1;
        appointment.DentistId = 2;
        appointment.AppointmentDateTime = appointmentDate;
        appointment.ProcedureType = "Cleaning";
        appointment.Notes = "Regular checkup";
        appointment.Status = AppointmentStatus.Completed;

        // Assert
        appointment.Id.Should().Be(1);
        appointment.CustomerId.Should().Be(1);
        appointment.DentistId.Should().Be(2);
        appointment.AppointmentDateTime.Should().Be(appointmentDate);
        appointment.ProcedureType.Should().Be("Cleaning");
        appointment.Notes.Should().Be("Regular checkup");
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Theory]
    [InlineData(AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.CancellationRequested)]
    public void Appointment_Status_ShouldAcceptAllValidStatuses(AppointmentStatus status)
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.Status = status;

        // Assert
        appointment.Status.Should().Be(status);
    }
}
