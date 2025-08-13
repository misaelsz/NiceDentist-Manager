using FluentAssertions;
using Moq;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.Services;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Application;

/// <summary>
/// Unit tests for AppointmentService (Working Version)
/// </summary>
public class AppointmentServiceWorkingTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository;
    private readonly Mock<IDentistRepository> _mockDentistRepository;
    private readonly AppointmentService _service;

    public AppointmentServiceWorkingTests()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockDentistRepository = new Mock<IDentistRepository>();
        _service = new AppointmentService(
            _mockAppointmentRepository.Object,
            _mockCustomerRepository.Object,
            _mockDentistRepository.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnAppointment()
    {
        // Arrange
        var appointmentId = 1;
        var appointment = new Appointment 
        { 
            Id = appointmentId, 
            CustomerId = 1, 
            DentistId = 1, 
            AppointmentDateTime = DateTime.UtcNow.AddDays(1),
            ProcedureType = "Cleaning",
            Status = AppointmentStatus.Scheduled
        };
        _mockAppointmentRepository.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        var result = await _service.GetByIdAsync(appointmentId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(appointmentId);
        _mockAppointmentRepository.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllAppointments()
    {
        // Arrange
        var appointments = new List<Appointment>
        {
            new Appointment { Id = 1, CustomerId = 1, DentistId = 1, AppointmentDateTime = DateTime.UtcNow.AddDays(1), ProcedureType = "Cleaning" },
            new Appointment { Id = 2, CustomerId = 2, DentistId = 1, AppointmentDateTime = DateTime.UtcNow.AddDays(2), ProcedureType = "Checkup" }
        };
        _mockAppointmentRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(appointments);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        _mockAppointmentRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidAppointment_ShouldCreateSuccessfully()
    {
        // Arrange
        var appointment = new Appointment 
        { 
            CustomerId = 1, 
            DentistId = 1, 
            AppointmentDateTime = DateTime.UtcNow.AddDays(1),
            ProcedureType = "Cleaning"
        };
        _mockAppointmentRepository.Setup(x => x.CreateAsync(appointment))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(appointment);

        // Assert
        result.Should().Be(1);
        _mockAppointmentRepository.Verify(x => x.CreateAsync(appointment), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidAppointment_ShouldUpdateSuccessfully()
    {
        // Arrange
        var appointment = new Appointment 
        { 
            Id = 1,
            CustomerId = 1, 
            DentistId = 1, 
            AppointmentDateTime = DateTime.UtcNow.AddDays(1),
            ProcedureType = "Cleaning"
        };
        _mockAppointmentRepository.Setup(x => x.UpdateAsync(appointment))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(appointment);

        // Assert
        _mockAppointmentRepository.Verify(x => x.UpdateAsync(appointment), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var appointmentId = 1;
        _mockAppointmentRepository.Setup(x => x.DeleteAsync(appointmentId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(appointmentId);

        // Assert
        _mockAppointmentRepository.Verify(x => x.DeleteAsync(appointmentId), Times.Once);
    }

    [Fact]
    public async Task CancelAppointmentAsync_WithValidId_ShouldCancelSuccessfully()
    {
        // Arrange
        var appointmentId = 1;
        var appointment = new Appointment 
        { 
            Id = appointmentId, 
            CustomerId = 1, 
            DentistId = 1, 
            AppointmentDateTime = DateTime.UtcNow.AddDays(1),
            ProcedureType = "Cleaning",
            Status = AppointmentStatus.Scheduled
        };
        
        _mockAppointmentRepository.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        _mockAppointmentRepository.Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CancelAppointmentAsync(appointmentId);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        _mockAppointmentRepository.Verify(x => x.GetByIdAsync(appointmentId), Times.Once);
        _mockAppointmentRepository.Verify(x => x.UpdateAsync(It.IsAny<Appointment>()), Times.Once);
    }
}
