using FluentAssertions;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Domain;

/// <summary>
/// Unit tests for Appointment domain entity (Fixed version)
/// </summary>
public class AppointmentTestsFixed
{
    [Fact]
    public void Appointment_Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var appointment = new Appointment();

        // Assert
        appointment.Id.Should().Be(0);
        appointment.CustomerId.Should().Be(0);
        appointment.DentistId.Should().Be(0);
        appointment.AppointmentDateTime.Should().Be(default);
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
        var appointmentDateTime = DateTime.UtcNow.AddDays(7);
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        appointment.Id = 1;
        appointment.CustomerId = 123;
        appointment.DentistId = 456;
        appointment.AppointmentDateTime = appointmentDateTime;
        appointment.ProcedureType = "Dental Cleaning";
        appointment.Notes = "Regular checkup and cleaning";
        appointment.Status = AppointmentStatus.Completed;
        appointment.CreatedAt = createdAt;
        appointment.UpdatedAt = updatedAt;

        // Assert
        appointment.Id.Should().Be(1);
        appointment.CustomerId.Should().Be(123);
        appointment.DentistId.Should().Be(456);
        appointment.AppointmentDateTime.Should().Be(appointmentDateTime);
        appointment.ProcedureType.Should().Be("Dental Cleaning");
        appointment.Notes.Should().Be("Regular checkup and cleaning");
        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.CreatedAt.Should().Be(createdAt);
        appointment.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Appointment_Status_ShouldSetToScheduledByDefault()
    {
        // Arrange & Act
        var appointment = new Appointment();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Appointment_Status_ShouldSetToCompleted()
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.Status = AppointmentStatus.Completed;

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Appointment_Status_ShouldSetToCancelled()
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.Status = AppointmentStatus.Cancelled;

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Appointment_Status_ShouldSetToCancellationRequested()
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.Status = AppointmentStatus.CancellationRequested;

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.CancellationRequested);
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
