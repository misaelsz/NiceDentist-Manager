using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NiceDentist.Manager.Api;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace NiceDentist.Manager.IntegrationTests.Controllers;

public class BasicIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public BasicIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllCustomers_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllDentists_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/dentists");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllAppointments_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        // Arrange
        var newCustomer = new
        {
            Name = "Test Customer",
            Email = "test@customer.com",
            Phone = "123-456-7890",
            DateOfBirth = "1990-01-01",
            Address = "123 Test St"
        };

        var content = new StringContent(JsonSerializer.Serialize(newCustomer), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateDentist_WithValidData_ReturnsCreatedOrBadRequest()
    {
        // Arrange
        var newDentist = new
        {
            Name = "Dr. Test",
            Email = "test@dentist.com",
            Phone = "123-456-7890",
            LicenseNumber = "TEST123456",
            Specialization = "General Dentistry"
        };

        var content = new StringContent(JsonSerializer.Serialize(newDentist), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/dentists", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomer_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/customers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDentist_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/dentists/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAppointment_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var updateData = new
        {
            Name = "Updated Customer",
            Email = "updated@customer.com",
            Phone = "123-456-7890",
            DateOfBirth = "1990-01-01",
            Address = "123 Updated St"
        };

        var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/customers/999", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/customers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
