using FluentAssertions;
using Moq;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.Services;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.Tests.Application;

/// <summary>
/// Unit tests for DentistService (Working Version)
/// </summary>
public class DentistServiceWorkingTests
{
    private readonly Mock<IDentistRepository> _mockDentistRepository;
    private readonly Mock<IAuthApiService> _mockAuthApiService;
    private readonly DentistService _service;

    public DentistServiceWorkingTests()
    {
        _mockDentistRepository = new Mock<IDentistRepository>();
        _mockAuthApiService = new Mock<IAuthApiService>();
        _service = new DentistService(_mockDentistRepository.Object, _mockAuthApiService.Object);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnDentist()
    {
        // Arrange
        var email = "dentist@example.com";
        var dentist = new Dentist { Id = 1, Email = email, FullName = "Dr. Test", LicenseNumber = "D12345" };
        _mockDentistRepository.Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(dentist);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        _mockDentistRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDentists()
    {
        // Arrange
        var dentists = new List<Dentist>
        {
            new Dentist { Id = 1, Email = "dentist1@example.com", FullName = "Dr. Test 1", LicenseNumber = "D12345" },
            new Dentist { Id = 2, Email = "dentist2@example.com", FullName = "Dr. Test 2", LicenseNumber = "D67890" }
        };
        _mockDentistRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(dentists);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        _mockDentistRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnDentist()
    {
        // Arrange
        var dentistId = 1;
        var dentist = new Dentist { Id = dentistId, Email = "dentist@example.com", FullName = "Dr. Test", LicenseNumber = "D12345" };
        _mockDentistRepository.Setup(x => x.GetByIdAsync(dentistId))
            .ReturnsAsync(dentist);

        // Act
        var result = await _service.GetByIdAsync(dentistId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(dentistId);
        _mockDentistRepository.Verify(x => x.GetByIdAsync(dentistId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidDentist_ShouldCreateSuccessfully()
    {
        // Arrange
        var dentist = new Dentist { Email = "dentist@example.com", FullName = "Dr. Test", LicenseNumber = "D12345" };
        _mockDentistRepository.Setup(x => x.CreateAsync(dentist))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dentist);

        // Assert
        result.Should().Be(1);
        _mockDentistRepository.Verify(x => x.CreateAsync(dentist), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var dentistId = 1;
        _mockDentistRepository.Setup(x => x.DeleteAsync(dentistId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(dentistId);

        // Assert
        _mockDentistRepository.Verify(x => x.DeleteAsync(dentistId), Times.Once);
    }
}
