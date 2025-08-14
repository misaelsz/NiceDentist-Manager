using Microsoft.Extensions.Logging;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Application.Events;

namespace NiceDentist.Manager.Application.EventHandlers;

/// <summary>
/// Handles UserCreated events from Auth API
/// Updates Customer/Dentist records with the UserId
/// </summary>
public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IDentistRepository _dentistRepository;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        ICustomerRepository customerRepository,
        IDentistRepository dentistRepository,
        ILogger<UserCreatedEventHandler> logger)
    {
        _customerRepository = customerRepository;
        _dentistRepository = dentistRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the UserCreated event by updating the corresponding Customer or Dentist with the UserId
    /// </summary>
    /// <param name="eventObject">UserCreated event</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if handled successfully</returns>
    public async Task<bool> HandleAsync(UserCreatedEvent eventObject, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing UserCreated event for email: {Email}, UserId: {UserId}, EntityType: {EntityType}", 
                eventObject.Data.Email, eventObject.Data.UserId, eventObject.Data.EntityType);

            switch (eventObject.Data.EntityType.ToLowerInvariant())
            {
                case "customer":
                    await UpdateCustomerWithUserIdAsync(eventObject.Data);
                    break;
                
                case "dentist":
                    await UpdateDentistWithUserIdAsync(eventObject.Data);
                    break;
                
                default:
                    _logger.LogWarning("Unknown entity type: {EntityType} for email: {Email}", 
                        eventObject.Data.EntityType, eventObject.Data.Email);
                    return false;
            }

            _logger.LogInformation("Successfully processed UserCreated event for email: {Email}", eventObject.Data.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UserCreated event for email: {Email}", eventObject.Data.Email);
            return false;
        }
    }

    /// <summary>
    /// Updates a customer record with the UserId from Auth API
    /// </summary>
    private async Task UpdateCustomerWithUserIdAsync(UserCreatedData userData)
    {
        // First try to find by EntityId, then fallback to Email
        var customer = await _customerRepository.GetByIdAsync(userData.EntityId);
        
        if (customer == null)
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found, trying to find by email: {Email}", 
                userData.EntityId, userData.Email);
            
            // Fallback: try to find by email
            var allCustomers = await _customerRepository.GetAllAsync();
            customer = allCustomers.FirstOrDefault(c => c.Email.Equals(userData.Email, StringComparison.OrdinalIgnoreCase));
            
            if (customer == null)
            {
                _logger.LogError("Customer with email {Email} not found in database", userData.Email);
                throw new InvalidOperationException($"Customer with email {userData.Email} not found");
            }
        }

        // Update customer with UserId
        customer.UserId = userData.UserId;
        await _customerRepository.UpdateAsync(customer);
        
        _logger.LogInformation("Updated Customer {CustomerId} with UserId {UserId}", customer.Id, userData.UserId);
    }

    /// <summary>
    /// Updates a dentist record with the UserId from Auth API
    /// </summary>
    private async Task UpdateDentistWithUserIdAsync(UserCreatedData userData)
    {
        // First try to find by EntityId, then fallback to Email
        var dentist = await _dentistRepository.GetByIdAsync(userData.EntityId);
        
        if (dentist == null)
        {
            _logger.LogWarning("Dentist with ID {DentistId} not found, trying to find by email: {Email}", 
                userData.EntityId, userData.Email);
            
            // Fallback: try to find by email
            var allDentists = await _dentistRepository.GetAllAsync();
            dentist = allDentists.FirstOrDefault(d => d.Email.Equals(userData.Email, StringComparison.OrdinalIgnoreCase));
            
            if (dentist == null)
            {
                _logger.LogError("Dentist with email {Email} not found in database", userData.Email);
                throw new InvalidOperationException($"Dentist with email {userData.Email} not found");
            }
        }

        // Update dentist with UserId
        dentist.UserId = userData.UserId;
        await _dentistRepository.UpdateAsync(dentist);
        
        _logger.LogInformation("Updated Dentist {DentistId} with UserId {UserId}", dentist.Id, userData.UserId);
    }
}
