using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Tests.Domain;

/// <summary>
/// Unit tests for Appointment domain entity
/// </summary>
public class AppointmentTests
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
        appointment.Status.Should().Be("Scheduled");
        appointment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        appointment.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        appointment.Customer.Should().BeNull();
        appointment.Dentist.Should().BeNull();
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
        appointment.Status = "Confirmed";
        appointment.CreatedAt = createdAt;
        appointment.UpdatedAt = updatedAt;

        // Assert
        appointment.Id.Should().Be(1);
        appointment.CustomerId.Should().Be(123);
        appointment.DentistId.Should().Be(456);
        appointment.AppointmentDateTime.Should().Be(appointmentDateTime);
        appointment.ProcedureType.Should().Be("Dental Cleaning");
        appointment.Notes.Should().Be("Regular checkup and cleaning");
        appointment.Status.Should().Be("Confirmed");
        appointment.CreatedAt.Should().Be(createdAt);
        appointment.UpdatedAt.Should().Be(updatedAt);
    }

    [Theory]
    [InlineData("Scheduled")]
    [InlineData("Confirmed")]
    [InlineData("In Progress")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    [InlineData("No Show")]
    [InlineData("Rescheduled")]
    public void Appointment_ValidStatuses_ShouldBeAccepted(string status)
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.Status = status;

        // Assert
        appointment.Status.Should().Be(status);
    }

    [Theory]
    [InlineData("Dental Cleaning")]
    [InlineData("Root Canal Treatment")]
    [InlineData("Teeth Whitening")]
    [InlineData("Dental Implant")]
    [InlineData("Orthodontic Consultation")]
    [InlineData("Emergency Treatment")]
    [InlineData("Routine Checkup")]
    [InlineData("X-Ray Examination")]
    [InlineData("Wisdom Tooth Extraction")]
    [InlineData("Braces Installation")]
    public void Appointment_ProcedureTypes_ShouldBeAccepted(string procedureType)
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.ProcedureType = procedureType;

        // Assert
        appointment.ProcedureType.Should().Be(procedureType);
    }

    [Theory]
    [InlineData("Patient has dental anxiety")]
    [InlineData("Follow-up appointment needed in 6 months")]
    [InlineData("Patient allergic to penicillin")]
    [InlineData("")]
    [InlineData("Requires X-ray before procedure")]
    [InlineData("Patient prefers morning appointments")]
    public void Appointment_Notes_ShouldAcceptAnyText(string notes)
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.Notes = notes;

        // Assert
        appointment.Notes.Should().Be(notes);
    }

    [Fact]
    public void Appointment_FutureDateTime_ShouldBeValid()
    {
        // Arrange
        var appointment = new Appointment();
        var futureDateTime = DateTime.UtcNow.AddDays(30);

        // Act
        appointment.AppointmentDateTime = futureDateTime;

        // Assert
        appointment.AppointmentDateTime.Should().Be(futureDateTime);
        appointment.AppointmentDateTime.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Appointment_PastDateTime_ShouldBeAccepted()
    {
        // Arrange
        var appointment = new Appointment();
        var pastDateTime = DateTime.UtcNow.AddDays(-5);

        // Act
        appointment.AppointmentDateTime = pastDateTime;

        // Assert
        appointment.AppointmentDateTime.Should().Be(pastDateTime);
        appointment.AppointmentDateTime.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Appointment_NavigationProperties_ShouldBeSettable()
    {
        // Arrange
        var appointment = new Appointment();
        var customer = new Customer { Id = 123, Name = "John Doe" };
        var dentist = new Dentist { Id = 456, Name = "Dr. Smith" };

        // Act
        appointment.Customer = customer;
        appointment.Dentist = dentist;

        // Assert
        appointment.Customer.Should().Be(customer);
        appointment.Dentist.Should().Be(dentist);
        appointment.Customer!.Id.Should().Be(123);
        appointment.Dentist!.Id.Should().Be(456);
    }

    [Fact]
    public void Appointment_CustomerAndDentistIds_ShouldBePositive()
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.CustomerId = 100;
        appointment.DentistId = 200;

        // Assert
        appointment.CustomerId.Should().BePositive();
        appointment.DentistId.Should().BePositive();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Appointment_InvalidCustomerId_ShouldBeHandled(int customerId)
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.CustomerId = customerId;

        // Assert
        appointment.CustomerId.Should().Be(customerId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Appointment_InvalidDentistId_ShouldBeHandled(int dentistId)
    {
        // Arrange
        var appointment = new Appointment();

        // Act
        appointment.DentistId = dentistId;

        // Assert
        appointment.DentistId.Should().Be(dentistId);
    }

    [Fact]
    public void Appointment_TimestampProperties_ShouldBeMaintained()
    {
        // Arrange
        var appointment = new Appointment();
        var createdTime = new DateTime(2023, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var updatedTime = new DateTime(2023, 1, 1, 10, 30, 0, DateTimeKind.Utc);

        // Act
        appointment.CreatedAt = createdTime;
        appointment.UpdatedAt = updatedTime;

        // Assert
        appointment.CreatedAt.Should().Be(createdTime);
        appointment.UpdatedAt.Should().Be(updatedTime);
        appointment.UpdatedAt.Should().BeAfter(appointment.CreatedAt);
    }

    [Fact]
    public void Appointment_StatusProgression_ShouldBeLogical()
    {
        // Arrange
        var appointment = new Appointment();

        // Act & Assert - Initial state
        appointment.Status.Should().Be("Scheduled");

        // Act - Confirm appointment
        appointment.Status = "Confirmed";
        appointment.Status.Should().Be("Confirmed");

        // Act - Start appointment
        appointment.Status = "In Progress";
        appointment.Status.Should().Be("In Progress");

        // Act - Complete appointment
        appointment.Status = "Completed";
        appointment.Status.Should().Be("Completed");
    }

    [Fact]
    public void Appointment_CancellationScenario_ShouldBeSupported()
    {
        // Arrange
        var appointment = new Appointment
        {
            Status = "Confirmed",
            Notes = "Initial appointment notes"
        };

        // Act
        appointment.Status = "Cancelled";
        appointment.Notes = "Cancelled due to patient illness";
        appointment.UpdatedAt = DateTime.UtcNow;

        // Assert
        appointment.Status.Should().Be("Cancelled");
        appointment.Notes.Should().Be("Cancelled due to patient illness");
    }
}
