using FluentAssertions;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Infrastructure.Services;
using Xunit;

namespace NiceDentist.Manager.Tests.Infrastructure;

/// <summary>
/// Unit tests for Mock services
/// </summary>
public class MockServicesTests
{
    [Fact]
    public async Task MockAuthApiService_ShouldReturnExpectedValues()
    {
        // Arrange
        var mockService = new MockAuthApiService();
        var request = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@email.com",
            Password = "password123",
            Role = "Customer"
        };

        // Act & Assert
        var userExists = await mockService.UserExistsByEmailAsync("test@email.com");
        userExists.Should().BeFalse();

        var createResult = await mockService.CreateUserAsync(request);
        createResult.Should().BeTrue();

        var deleteResult = await mockService.DeleteUserByEmailAsync("test@email.com");
        deleteResult.Should().BeTrue();
    }

    [Fact]
    public async Task MockEmailService_ShouldCompleteSuccessfully()
    {
        // Arrange
        var mockService = new MockEmailService();

        // Act
        var sendWelcomeTask = mockService.SendWelcomeEmailAsync(
            "john@email.com", 
            "John Doe", 
            "johndoe", 
            "password123", 
            "Customer");

        // Assert
        await sendWelcomeTask; // Should complete without exception
        sendWelcomeTask.IsCompletedSuccessfully.Should().BeTrue();
    }
}
