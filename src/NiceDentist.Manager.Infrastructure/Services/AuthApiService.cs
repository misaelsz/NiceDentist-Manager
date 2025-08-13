using System.Text;
using System.Text.Json;
using NiceDentist.Manager.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NiceDentist.Manager.Infrastructure.Services;

/// <summary>
/// Configuration options for Auth API
/// </summary>
public class AuthApiOptions
{
    /// <summary>
    /// Gets or sets the base URL of the Auth API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key for authentication
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout for HTTP requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// DTOs for Auth API communication
/// </summary>
public class AuthApiRegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class AuthApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class AuthApiCheckUserResponse
{
    public bool Exists { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}

/// <summary>
/// Implementation of Auth API service using HttpClient
/// </summary>
public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiService> _logger;
    private readonly AuthApiOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the AuthApiService
    /// </summary>
    /// <param name="httpClient">HTTP client for API calls</param>
    /// <param name="logger">Logger instance</param>
    /// <param name="options">Auth API configuration options</param>
    public AuthApiService(
        HttpClient httpClient,
        ILogger<AuthApiService> logger,
        IOptions<AuthApiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        
        // Add API key to headers if provided
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
        }

        // Configure JSON serialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public async Task<bool> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Creating user in Auth API for email: {Email}", request.Email);

            var authRequest = new AuthApiRegisterRequest
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password,
                Role = request.Role
            };

            var json = JsonSerializer.Serialize(authRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthApiResponse>(responseContent, _jsonOptions);

                if (authResponse?.Success == true)
                {
                    _logger.LogInformation("User created successfully in Auth API for email: {Email}", request.Email);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Auth API returned success=false for user creation. Message: {Message}", 
                        authResponse?.Message);
                    return false;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create user in Auth API. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception while creating user in Auth API for email: {Email}", request.Email);
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout while creating user in Auth API for email: {Email}", request.Email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating user in Auth API for email: {Email}", request.Email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteUserByEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Deleting user from Auth API for email: {Email}", email);

            var response = await _httpClient.DeleteAsync($"/api/auth/user/email/{Uri.EscapeDataString(email)}");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("User deleted successfully from Auth API for email: {Email}", email);
                return true;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("User not found in Auth API for email: {Email}", email);
                return true; // Consider as success since user doesn't exist
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to delete user from Auth API. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception while deleting user from Auth API for email: {Email}", email);
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout while deleting user from Auth API for email: {Email}", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting user from Auth API for email: {Email}", email);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        try
        {
            _logger.LogDebug("Checking if user exists in Auth API for email: {Email}", email);

            var response = await _httpClient.GetAsync($"/api/auth/user/email/{Uri.EscapeDataString(email)}/exists");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var checkResponse = JsonSerializer.Deserialize<AuthApiCheckUserResponse>(responseContent, _jsonOptions);

                var exists = checkResponse?.Exists ?? false;
                _logger.LogDebug("User existence check completed for email: {Email}, Exists: {Exists}", email, exists);
                return exists;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("User does not exist in Auth API for email: {Email}", email);
                return false;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to check user existence in Auth API. Status: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                return false; // Default to false on error
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception while checking user existence in Auth API for email: {Email}", email);
            return false;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timeout while checking user existence in Auth API for email: {Email}", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking user existence in Auth API for email: {Email}", email);
            return false;
        }
    }
}
