using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NiceDentist.Manager.Api.DTOs.Responses;
using NiceDentist.Manager.Application.DTOs;
using Xunit;

namespace NiceDentist.Manager.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for DentistsController
/// </summary>
public class DentistsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public DentistsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllDentists_ReturnsOkWithPagedResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/dentists");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<DentistResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        pagedResponse.Should().NotBeNull();
        pagedResponse!.Data.Should().NotBeEmpty();
        pagedResponse.Data.Should().HaveCountGreaterThan(0);
        pagedResponse.Total.Should().BeGreaterThan(0);
        pagedResponse.Page.Should().Be(1);
        pagedResponse.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllDentists_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/dentists?page=1&pageSize=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<DentistResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        pagedResponse.Should().NotBeNull();
        pagedResponse!.Data.Should().HaveCount(1);
        pagedResponse.Page.Should().Be(1);
        pagedResponse.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task GetAllDentists_WithSearchTerm_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/dentists?search=Johnson");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<DentistResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        pagedResponse.Should().NotBeNull();
        pagedResponse!.Data.Should().NotBeEmpty();
        pagedResponse.Data.Should().Contain(d => d.Name.Contains("Johnson", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetDentist_ExistingId_ReturnsOkWithDentist()
    {
        // Act
        var response = await _client.GetAsync("/api/dentists/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var dentist = JsonSerializer.Deserialize<DentistResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dentist.Should().NotBeNull();
        dentist!.Id.Should().Be(1);
        dentist.Name.Should().NotBeNullOrEmpty();
        dentist.Email.Should().NotBeNullOrEmpty();
        dentist.LicenseNumber.Should().NotBeNullOrEmpty();
        dentist.Specialization.Should().NotBeNullOrEmpty();
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
    public async Task GetDentistByEmail_ExistingEmail_ReturnsOkWithDentist()
    {
        // Act
        var response = await _client.GetAsync("/api/dentists/by-email/dr.johnson@nicedentist.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var dentist = JsonSerializer.Deserialize<DentistResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dentist.Should().NotBeNull();
        dentist!.Email.Should().Be("dr.johnson@nicedentist.com");
        dentist.Name.Should().NotBeNullOrEmpty();
        dentist.LicenseNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetDentistByEmail_NonExistingEmail_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/dentists/by-email/nonexistent@example.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateDentist_ValidData_ReturnsCreatedWithDentist()
    {
        // Arrange
        var request = new DentistDto
        {
            Name = "Dr. Test Dentist",
            Email = "dr.test.dentist@nicedentist.com",
            Phone = "123-456-7890",
            LicenseNumber = "DDS999999",
            Specialization = "Oral Surgery",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/dentists", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var dentist = JsonSerializer.Deserialize<DentistResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dentist.Should().NotBeNull();
        dentist!.Name.Should().Be(request.Name);
        dentist.Email.Should().Be(request.Email);
        dentist.Phone.Should().Be(request.Phone);
        dentist.LicenseNumber.Should().Be(request.LicenseNumber);
        dentist.Specialization.Should().Be(request.Specialization);
        dentist.IsActive.Should().BeTrue();

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/dentists/{dentist.Id}");
    }

    [Fact]
    public async Task CreateDentist_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = new DentistDto
        {
            Name = "Dr. Duplicate Email",
            Email = "dr.johnson@nicedentist.com", // This email already exists in seed data
            Phone = "123-456-7890",
            LicenseNumber = "DDS888888",
            Specialization = "General Dentistry",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/dentists", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("email");
        responseContent.Should().Contain("DUPLICATE_EMAIL");
    }

    [Fact]
    public async Task CreateDentist_DuplicateLicenseNumber_ReturnsConflict()
    {
        // Arrange
        var request = new DentistDto
        {
            Name = "Dr. Duplicate License",
            Email = "dr.duplicate.license@nicedentist.com",
            Phone = "123-456-7890",
            LicenseNumber = "DDS123456", // This license number already exists in seed data
            Specialization = "General Dentistry",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/dentists", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("license");
        responseContent.Should().Contain("DUPLICATE_LICENSE");
    }

    [Fact]
    public async Task CreateDentist_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new DentistDto
        {
            Name = "", // Invalid empty name
            Email = "invalid-email", // Invalid email format
            Phone = "123-456-7890",
            LicenseNumber = "",
            Specialization = "General Dentistry",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/dentists", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDentist_ExistingDentist_ReturnsOkWithUpdatedDentist()
    {
        // Arrange
        var request = new DentistDto
        {
            Name = "Dr. Updated Johnson",
            Email = "dr.updated.johnson@nicedentist.com",
            Phone = "999-888-7777",
            LicenseNumber = "DDS123456",
            Specialization = "Cosmetic Dentistry",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/dentists/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var dentist = JsonSerializer.Deserialize<DentistResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        dentist.Should().NotBeNull();
        dentist!.Id.Should().Be(1);
        dentist.Name.Should().Be(request.Name);
        dentist.Email.Should().Be(request.Email);
        dentist.Phone.Should().Be(request.Phone);
        dentist.Specialization.Should().Be(request.Specialization);
        dentist.IsActive.Should().Be(request.IsActive);
    }

    [Fact]
    public async Task UpdateDentist_NonExistingDentist_ReturnsBadRequest()
    {
        // Arrange
        var request = new DentistDto
        {
            Name = "Dr. Non-existing",
            Email = "dr.nonexisting@nicedentist.com",
            Phone = "123-456-7890",
            LicenseNumber = "DDS777777",
            Specialization = "General Dentistry",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/dentists/999", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateDentist_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new DentistDto
        {
            Name = "", // Invalid empty name
            Email = "invalid-email", // Invalid email format
            Phone = "123-456-7890",
            LicenseNumber = "DDS123456",
            Specialization = "General Dentistry",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/dentists/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteDentist_ExistingDentist_ReturnsNoContent()
    {
        // First, create a dentist to delete
        var createRequest = new DentistDto
        {
            Name = "Dr. Delete Test",
            Email = "dr.delete.test@nicedentist.com",
            Phone = "123-456-7890",
            LicenseNumber = "DDS666666",
            Specialization = "General Dentistry",
            IsActive = true
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/dentists", createContent);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdDentist = JsonSerializer.Deserialize<DentistResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Delete the dentist
        var deleteResponse = await _client.DeleteAsync($"/api/dentists/{createdDentist!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the dentist is deleted
        var getResponse = await _client.GetAsync($"/api/dentists/{createdDentist.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteDentist_NonExistingDentist_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/dentists/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DentistWorkflow_CreateReadUpdateDelete_WorksCorrectly()
    {
        // Create
        var createRequest = new DentistDto
        {
            Name = "Dr. Workflow Test",
            Email = "dr.workflow.test@nicedentist.com",
            Phone = "123-456-7890",
            LicenseNumber = "DDS555555",
            Specialization = "Periodontics",
            IsActive = true
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/dentists", createContent);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdDentist = JsonSerializer.Deserialize<DentistResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        createdDentist.Should().NotBeNull();

        // Read
        var readResponse = await _client.GetAsync($"/api/dentists/{createdDentist!.Id}");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readResponseContent = await readResponse.Content.ReadAsStringAsync();
        var readDentist = JsonSerializer.Deserialize<DentistResponse>(readResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        readDentist.Should().NotBeNull();
        readDentist!.Name.Should().Be(createRequest.Name);

        // Update
        var updateRequest = new DentistDto
        {
            Name = "Dr. Updated Workflow Test",
            Email = "dr.updated.workflow.test@nicedentist.com",
            Phone = "999-888-7777",
            LicenseNumber = createRequest.LicenseNumber,
            Specialization = "Endodontics",
            IsActive = true
        };

        var updateJson = JsonSerializer.Serialize(updateRequest);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
        var updateResponse = await _client.PutAsync($"/api/dentists/{createdDentist.Id}", updateContent);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var updatedDentist = JsonSerializer.Deserialize<DentistResponse>(updateResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        updatedDentist.Should().NotBeNull();
        updatedDentist!.Name.Should().Be(updateRequest.Name);
        updatedDentist.Email.Should().Be(updateRequest.Email);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/dentists/{createdDentist.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/dentists/{createdDentist.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
