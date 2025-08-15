using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NiceDentist.Manager.Api.DTOs.Requests;
using NiceDentist.Manager.Api.DTOs.Responses;
using NiceDentist.Manager.Domain;
using Xunit;

namespace NiceDentist.Manager.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for AppointmentsController
/// </summary        appointment!.Status.Should().Be("Scheduled");
public class AppointmentsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public AppointmentsControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllAppointments_ReturnsOkWithAppointments()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointments = JsonSerializer.Deserialize<IEnumerable<AppointmentResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointments.Should().NotBeNull();
        appointments.Should().NotBeEmpty();
        appointments.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAllAppointments_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments?page=1&pageSize=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointments = JsonSerializer.Deserialize<IEnumerable<AppointmentResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointments.Should().NotBeNull();
        appointments.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAppointments_WithCustomerFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments?customerId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointments = JsonSerializer.Deserialize<IEnumerable<AppointmentResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointments.Should().NotBeNull();
        appointments.Should().OnlyContain(a => a.CustomerId == 1);
    }

    [Fact]
    public async Task GetAllAppointments_WithDentistFilter_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments?dentistId=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointments = JsonSerializer.Deserialize<IEnumerable<AppointmentResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointments.Should().NotBeNull();
        appointments.Should().OnlyContain(a => a.DentistId == 1);
    }

    [Fact]
    public async Task GetAppointmentById_ExistingId_ReturnsOkWithAppointment()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointment.Should().NotBeNull();
        appointment!.Id.Should().Be(1);
        appointment.CustomerId.Should().BeGreaterThan(0);
        appointment.DentistId.Should().BeGreaterThan(0);
        appointment.CustomerName.Should().NotBeNullOrEmpty();
        appointment.DentistName.Should().NotBeNullOrEmpty();
        appointment.AppointmentDateTime.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GetAppointmentById_NonExistingId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAppointmentsByCustomer_ExistingCustomer_ReturnsOkWithAppointments()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments/customer/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointments = JsonSerializer.Deserialize<IEnumerable<AppointmentResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointments.Should().NotBeNull();
        appointments.Should().OnlyContain(a => a.CustomerId == 1);
    }

    [Fact]
    public async Task GetAppointmentsByDentist_ExistingDentist_ReturnsOkWithAppointments()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments/dentist/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var appointments = JsonSerializer.Deserialize<IEnumerable<AppointmentResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointments.Should().NotBeNull();
        appointments.Should().OnlyContain(a => a.DentistId == 1);
    }

    [Fact]
    public async Task GetAvailableSlots_ValidDateRange_ReturnsOkWithSlots()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(2);

        // Act
        var response = await _client.GetAsync($"/api/appointments/dentist/1/available-slots?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var slots = JsonSerializer.Deserialize<IEnumerable<AvailableSlotResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        slots.Should().NotBeNull();
        slots.Should().OnlyContain(s => s.DentistId == 1);
        slots.Should().OnlyContain(s => s.IsAvailable == true);
    }

    [Fact]
    public async Task GetAvailableSlots_InvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(2);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var response = await _client.GetAsync($"/api/appointments/dentist/1/available-slots?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllAvailableSlots_ValidDateRange_ReturnsOkWithSlots()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow.AddDays(2);

        // Act
        var response = await _client.GetAsync($"/api/appointments/available-slots?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var slots = JsonSerializer.Deserialize<IEnumerable<AvailableSlotResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        slots.Should().NotBeNull();
        slots.Should().OnlyContain(s => s.IsAvailable == true);
    }

    [Fact]
    public async Task CreateAppointment_ValidData_ReturnsCreatedWithAppointment()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(30);
        var request = new CreateAppointmentRequest
        {
            CustomerId = 1,
            DentistId = 1,
            AppointmentDateTime = futureDate,
            ProcedureType = "Routine Cleaning",
            Notes = "Regular cleaning appointment"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/appointments", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointment.Should().NotBeNull();
        appointment!.CustomerId.Should().Be(request.CustomerId);
        appointment.DentistId.Should().Be(request.DentistId);
        appointment.AppointmentDateTime.Should().BeCloseTo(request.AppointmentDateTime, TimeSpan.FromSeconds(1));
        appointment.ProcedureType.Should().Be(request.ProcedureType);
        appointment.Notes.Should().Be(request.Notes);
        appointment.Status.Should().Be("Scheduled");

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/appointments/{appointment.Id}");
    }

    [Fact]
    public async Task CreateAppointment_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateAppointmentRequest
        {
            CustomerId = 0, // Invalid customer ID
            DentistId = 0, // Invalid dentist ID
            AppointmentDateTime = DateTime.UtcNow.AddDays(-1), // Past date
            ProcedureType = "",
            Notes = ""
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/appointments", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateAppointment_ExistingAppointment_ReturnsOkWithUpdatedAppointment()
    {
        // First, create an appointment
        var futureDate = DateTime.UtcNow.AddDays(45);
        var createRequest = new CreateAppointmentRequest
        {
            CustomerId = 2,
            DentistId = 2,
            AppointmentDateTime = futureDate,
            ProcedureType = "Initial Consultation",
            Notes = "First visit"
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/appointments", createContent);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdAppointment = JsonSerializer.Deserialize<AppointmentResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Arrange - Update request
        var updateRequest = new UpdateAppointmentRequest
        {
            CustomerId = 2,
            DentistId = 2,
            AppointmentDateTime = futureDate.AddHours(2),
            ProcedureType = "Updated Consultation",
            Notes = "Updated notes"
        };

        var updateJson = JsonSerializer.Serialize(updateRequest);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/appointments/{createdAppointment!.Id}", updateContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointment.Should().NotBeNull();
        appointment!.Id.Should().Be(createdAppointment.Id);
        appointment.ProcedureType.Should().Be(updateRequest.ProcedureType);
        appointment.Notes.Should().Be(updateRequest.Notes);
    }

    [Fact]
    public async Task UpdateAppointment_NonExistingAppointment_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateAppointmentRequest
        {
            CustomerId = 1,
            DentistId = 1,
            AppointmentDateTime = DateTime.UtcNow.AddDays(1),
            ProcedureType = "Test",
            Notes = "Test"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync("/api/appointments/999", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAppointmentStatus_ExistingAppointment_ReturnsOkWithUpdatedStatus()
    {
        // First, create an appointment
        var futureDate = DateTime.UtcNow.AddDays(50);
        var createRequest = new CreateAppointmentRequest
        {
            CustomerId = 1,
            DentistId = 1,
            AppointmentDateTime = futureDate,
            ProcedureType = "Status Test",
            Notes = "Status test appointment"
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/appointments", createContent);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdAppointment = JsonSerializer.Deserialize<AppointmentResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Arrange - Status update request
        var request = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Scheduled,
            Reason = "Patient confirmed attendance"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/appointments/{createdAppointment!.Id}/status", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointment.Should().NotBeNull();
        appointment!.Id.Should().Be(createdAppointment.Id);
        appointment.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task CancelAppointment_ExistingAppointment_ReturnsOkWithCancelledStatus()
    {
        // First, create an appointment
        var futureDate = DateTime.UtcNow.AddDays(55);
        var createRequest = new CreateAppointmentRequest
        {
            CustomerId = 1,
            DentistId = 1,
            AppointmentDateTime = futureDate,
            ProcedureType = "Cancel Test",
            Notes = "Cancel test appointment"
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/appointments", createContent);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdAppointment = JsonSerializer.Deserialize<AppointmentResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Arrange - Cancellation request
        var request = new CancelAppointmentRequest
        {
            Reason = "Patient emergency"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/appointments/{createdAppointment!.Id}/cancel", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointment.Should().NotBeNull();
        appointment!.Id.Should().Be(createdAppointment.Id);
        appointment.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CompleteAppointment_ExistingAppointment_ReturnsOkWithCompletedStatus()
    {
        // First, create an appointment
        var futureDate = DateTime.UtcNow.AddDays(60);
        var createRequest = new CreateAppointmentRequest
        {
            CustomerId = 1,
            DentistId = 1,
            AppointmentDateTime = futureDate,
            ProcedureType = "Complete Test",
            Notes = "Complete test appointment"
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/appointments", createContent);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdAppointment = JsonSerializer.Deserialize<AppointmentResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Arrange - Completion request
        var request = new CompleteAppointmentRequest
        {
            Notes = "Appointment completed successfully"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/appointments/{createdAppointment!.Id}/complete", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        appointment.Should().NotBeNull();
        appointment!.Id.Should().Be(createdAppointment.Id);
        appointment.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task DeleteAppointment_ExistingAppointment_ReturnsNoContent()
    {
        // First, create an appointment to delete
        var futureDate = DateTime.UtcNow.AddDays(65);
        var createRequest = new CreateAppointmentRequest
        {
            CustomerId = 2,
            DentistId = 2,
            AppointmentDateTime = futureDate,
            ProcedureType = "Delete Test",
            Notes = "Delete test appointment"
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/appointments", createContent);

        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdAppointment = JsonSerializer.Deserialize<AppointmentResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Act - Delete the appointment
        var deleteResponse = await _client.DeleteAsync($"/api/appointments/{createdAppointment!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify the appointment is deleted
        var getResponse = await _client.GetAsync($"/api/appointments/{createdAppointment.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAppointment_NonExistingAppointment_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/appointments/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AppointmentWorkflow_CreateUpdateStatusDelete_WorksCorrectly()
    {
        // Create
        var futureDate = DateTime.UtcNow.AddDays(70);
        var createRequest = new CreateAppointmentRequest
        {
            CustomerId = 2,
            DentistId = 2,
            AppointmentDateTime = futureDate,
            ProcedureType = "Workflow Test",
            Notes = "Workflow test appointment"
        };

        var createJson = JsonSerializer.Serialize(createRequest);
        var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync("/api/appointments", createContent);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdAppointment = JsonSerializer.Deserialize<AppointmentResponse>(createResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        createdAppointment.Should().NotBeNull();

        // Read
        var readResponse = await _client.GetAsync($"/api/appointments/{createdAppointment!.Id}");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readResponseContent = await readResponse.Content.ReadAsStringAsync();
        var readAppointment = JsonSerializer.Deserialize<AppointmentResponse>(readResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        readAppointment.Should().NotBeNull();
        readAppointment!.ProcedureType.Should().Be(createRequest.ProcedureType);

        // Update Status to Scheduled
        var statusRequest = new UpdateAppointmentStatusRequest
        {
            Status = AppointmentStatus.Scheduled,
            Reason = "Patient confirmed"
        };

        var statusJson = JsonSerializer.Serialize(statusRequest);
        var statusContent = new StringContent(statusJson, Encoding.UTF8, "application/json");
        var statusResponse = await _client.PutAsync($"/api/appointments/{createdAppointment.Id}/status", statusContent);

        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusResponseContent = await statusResponse.Content.ReadAsStringAsync();
        var confirmedAppointment = JsonSerializer.Deserialize<AppointmentResponse>(statusResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        confirmedAppointment.Should().NotBeNull();
        confirmedAppointment!.Status.Should().Be("Scheduled");

        // Complete the appointment
        var completeRequest = new CompleteAppointmentRequest
        {
            Notes = "Successfully completed"
        };

        var completeJson = JsonSerializer.Serialize(completeRequest);
        var completeContent = new StringContent(completeJson, Encoding.UTF8, "application/json");
        var completeResponse = await _client.PutAsync($"/api/appointments/{createdAppointment.Id}/complete", completeContent);

        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completeResponseContent = await completeResponse.Content.ReadAsStringAsync();
        var completedAppointment = JsonSerializer.Deserialize<AppointmentResponse>(completeResponseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        completedAppointment.Should().NotBeNull();
        completedAppointment!.Status.Should().Be("Completed");

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/appointments/{createdAppointment.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var verifyResponse = await _client.GetAsync($"/api/appointments/{createdAppointment.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
