using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NiceDentist.Manager.Api.DTOs.Requests;
using NiceDentist.Manager.Api.DTOs.Responses;
using Xunit;

namespace NiceDentist.Manager.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for CustomersController
/// </summary>
public class CustomersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public CustomersControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllCustomers_ReturnsOkWithPagedResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<CustomerResponse>>(content, new JsonSerializerOptions
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
    public async Task GetAllCustomers_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/customers?page=1&pageSize=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<CustomerResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        pagedResponse.Should().NotBeNull();
        pagedResponse!.Data.Should().HaveCount(1);
        pagedResponse.Page.Should().Be(1);
        pagedResponse.PageSize.Should().Be(1);
    }

    [Fact]
    public async Task GetAllCustomers_WithSearchTerm_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/customers?search=John");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var pagedResponse = JsonSerializer.Deserialize<PagedResponse<CustomerResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        pagedResponse.Should().NotBeNull();
        pagedResponse!.Data.Should().NotBeEmpty();
        pagedResponse.Data.Should().Contain(c => c.Name.Contains("John", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetCustomerById_ExistingId_ReturnsOkWithCustomer()
    {
        // Act
        var response = await _client.GetAsync("/api/customers/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customer.Should().NotBeNull();
        customer!.Id.Should().Be(1);
        customer.Name.Should().NotBeNullOrEmpty();
        customer.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCustomerById_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/customers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCustomer_ValidData_ReturnsCreatedWithCustomer()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "Test Customer",
            Email = "test.customer@example.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1995, 5, 15),
            Address = "123 Test St"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customer.Should().NotBeNull();
        customer!.Name.Should().Be(request.Name);
        customer.Email.Should().Be(request.Email);
        customer.Phone.Should().Be(request.Phone);
        customer.DateOfBirth.Should().Be(request.DateOfBirth);
        customer.Address.Should().Be(request.Address);
        customer.IsActive.Should().BeTrue();

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/customers/{customer.Id}");
    }

    [Fact]
    public async Task CreateCustomer_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "Duplicate Customer",
            Email = "john.doe@example.com", // This email already exists in seed data
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1995, 5, 15),
            Address = "123 Test St"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("email");
        responseContent.Should().Contain("DUPLICATE_EMAIL");
    }

    [Fact]
    public async Task CreateCustomer_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateCustomerRequest
        {
            Name = "", // Invalid empty name
            Email = "invalid-email", // Invalid email format
            Phone = "123-456-7890"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/customers", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCustomer_ExistingCustomer_ReturnsOkWithUpdatedCustomer()
    {
        // Arrange
        var request = new UpdateCustomerRequest
        {
            Name = "Updated John Doe",
            Email = "updated.john.doe@example.com",
            Phone = "999-888-7777",
            DateOfBirth = new DateTime(1990, 1, 15),
            Address = "456 Updated St",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/customers/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        customer.Should().NotBeNull();
        customer!.Id.Should().Be(1);
        customer.Name.Should().Be(request.Name);
        customer.Email.Should().Be(request.Email);
        customer.Phone.Should().Be(request.Phone);
        customer.Address.Should().Be(request.Address);
        customer.IsActive.Should().Be(request.IsActive);
    }

    [Fact]
    public async Task UpdateCustomer_NonExistingCustomer_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateCustomerRequest
        {
            Name = "Non-existing Customer",
            Email = "nonexisting@example.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Non-existing St",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/customers/999", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new UpdateCustomerRequest
        {
            Name = "", // Invalid empty name
            Email = "invalid-email", // Invalid email format
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Test St",
            IsActive = true
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/customers/1", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteCustomer_ExistingCustomer_ReturnsNoContent()
    {
        // First, create a customer to delete
        var createRequest = new CreateCustomerRequest
        {
            Name = "Customer To Delete",
            Email = "delete.customer@example.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Delete St"
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/customers", createContent);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCustomer = JsonSerializer.Deserialize<CustomerResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Delete the customer
        var deleteResponse = await _client.DeleteAsync($"/api/customers/{createdCustomer!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the customer is deleted
        var getResponse = await _client.GetAsync($"/api/customers/{createdCustomer.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_NonExistingCustomer_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/customers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CustomerWorkflow_CreateReadUpdateDelete_WorksCorrectly()
    {
        // Create
        var createRequest = new CreateCustomerRequest
        {
            Name = "Workflow Test Customer",
            Email = "workflow.test@example.com",
            Phone = "123-456-7890",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Workflow St"
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/customers", createContent);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCustomer = JsonSerializer.Deserialize<CustomerResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        createdCustomer.Should().NotBeNull();

        // Read
        var readResponse = await _client.GetAsync($"/api/customers/{createdCustomer!.Id}");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readResponseContent = await readResponse.Content.ReadAsStringAsync();
        var readCustomer = JsonSerializer.Deserialize<CustomerResponse>(readResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        readCustomer.Should().NotBeNull();
        readCustomer!.Name.Should().Be(createRequest.Name);

        // Update
        var updateRequest = new UpdateCustomerRequest
        {
            Name = "Updated Workflow Customer",
            Email = "updated.workflow.test@example.com",
            Phone = "999-888-7777",
            DateOfBirth = createRequest.DateOfBirth,
            Address = "456 Updated Workflow St",
            IsActive = true
        };

        var updateJson = JsonSerializer.Serialize(updateRequest);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
        var updateResponse = await _client.PutAsync($"/api/customers/{createdCustomer.Id}", updateContent);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        var updatedCustomer = JsonSerializer.Deserialize<CustomerResponse>(updateResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.Name.Should().Be(updateRequest.Name);
        updatedCustomer.Email.Should().Be(updateRequest.Email);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/customers/{createdCustomer.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/customers/{createdCustomer.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
