using Xunit;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Tests.Domain
{
    /// <summary>
    /// Tests for Appointment domain entity
    /// Testing business rules and entity behavior
    /// </summary>
    public class AppointmentDomainTests
    {
        [Fact]
        public void Appointment_CreateWithValidData_ShouldHaveCorrectProperties()
        {
            // Arrange
            var customerId = 1;
            var dentistId = 1;
            var appointmentDateTime = DateTime.Now.AddDays(1);
            var procedureType = "Cleaning";
            var notes = "Regular checkup";
            var status = AppointmentStatus.Scheduled;

            // Act
            var appointment = new Appointment
            {
                CustomerId = customerId,
                DentistId = dentistId,
                AppointmentDateTime = appointmentDateTime,
                ProcedureType = procedureType,
                Notes = notes,
                Status = status
            };

            // Assert
            Assert.Equal(customerId, appointment.CustomerId);
            Assert.Equal(dentistId, appointment.DentistId);
            Assert.Equal(appointmentDateTime, appointment.AppointmentDateTime);
            Assert.Equal(procedureType, appointment.ProcedureType);
            Assert.Equal(notes, appointment.Notes);
            Assert.Equal(status, appointment.Status);
            Assert.True(appointment.CreatedAt <= DateTime.UtcNow);
            Assert.True(appointment.UpdatedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Appointment_DefaultValues_ShouldBeCorrect()
        {
            // Act
            var appointment = new Appointment();

            // Assert
            Assert.Equal(0, appointment.CustomerId);
            Assert.Equal(0, appointment.DentistId);
            Assert.Equal(string.Empty, appointment.ProcedureType);
            Assert.Equal(string.Empty, appointment.Notes);
            Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        }

        [Fact]
        public void AppointmentStatus_AllValues_ShouldBeValid()
        {
            // Act & Assert
            Assert.Equal(1, (int)AppointmentStatus.Scheduled);
            Assert.Equal(2, (int)AppointmentStatus.Completed);
            Assert.Equal(3, (int)AppointmentStatus.Cancelled);
            Assert.Equal(4, (int)AppointmentStatus.CancellationRequested);
        }

        [Fact]
        public void Appointment_ChangeStatus_ShouldUpdateCorrectly()
        {
            // Arrange
            var appointment = new Appointment { Status = AppointmentStatus.Scheduled };

            // Act
            appointment.Status = AppointmentStatus.Completed;

            // Assert
            Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        }

        [Fact]
        public void Appointment_UpdateDateTime_ShouldChangeUpdatedAt()
        {
            // Arrange
            var appointment = new Appointment();
            var originalUpdatedAt = appointment.UpdatedAt;
            Thread.Sleep(1); // Ensure time difference

            // Act
            appointment.UpdatedAt = DateTime.UtcNow;

            // Assert
            Assert.True(appointment.UpdatedAt > originalUpdatedAt);
        }
    }
}
