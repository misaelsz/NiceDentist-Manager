using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Infrastructure.Repositories;

/// <summary>
/// SQL Server implementation of customer repository
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found");
    }

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="customer">The customer to create</param>
    /// <returns>The created customer with assigned ID</returns>
    public async Task<Customer> CreateAsync(Customer customer)
    {
        const string sql = @"
            INSERT INTO Customers (Name, Email, Phone, DateOfBirth, Address, CreatedAt, UpdatedAt, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Email, @Phone, @DateOfBirth, @Address, @CreatedAt, @UpdatedAt, @IsActive)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Name", customer.Name);
        command.Parameters.AddWithValue("@Email", customer.Email);
        command.Parameters.AddWithValue("@Phone", customer.Phone);
        command.Parameters.AddWithValue("@DateOfBirth", customer.DateOfBirth == default ? (object)DBNull.Value : customer.DateOfBirth);
        command.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@IsActive", customer.IsActive);

        var id = await command.ExecuteScalarAsync();
        customer.Id = Convert.ToInt32(id);
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        return customer;
    }

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>The customer if found, null otherwise</returns>
    public async Task<Customer?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Name, Email, Phone, DateOfBirth, Address, CreatedAt, UpdatedAt, IsActive
            FROM Customers 
            WHERE Id = @Id AND IsActive = 1";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Gets a customer by email
    /// </summary>
    /// <param name="email">The customer email</param>
    /// <returns>The customer if found, null otherwise</returns>
    public async Task<Customer?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT Id, Name, Email, Phone, DateOfBirth, Address, CreatedAt, UpdatedAt, IsActive
            FROM Customers 
            WHERE Email = @Email AND IsActive = 1";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Gets all customers with pagination and optional search
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Optional search term</param>
    /// <returns>List of customers</returns>
    public async Task<IEnumerable<Customer>> GetAllAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        var sql = @"
            SELECT Id, Name, Email, Phone, DateOfBirth, Address, CreatedAt, UpdatedAt, IsActive
            FROM Customers 
            WHERE IsActive = 1";

        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql += @" AND (
                Name LIKE @Search OR 
                Email LIKE @Search OR 
                Phone LIKE @Search
            )";
            parameters.Add(new SqlParameter("@Search", $"%{search}%"));
        }

        sql += @"
            ORDER BY Name
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add(new SqlParameter("@Offset", (page - 1) * pageSize));
        parameters.Add(new SqlParameter("@PageSize", pageSize));

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());

        var customers = new List<Customer>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(MapFromReader(reader));
        }

        return customers;
    }

    /// <summary>
    /// Updates a customer
    /// </summary>
    /// <param name="customer">The customer to update</param>
    /// <returns>The updated customer</returns>
    public async Task<Customer> UpdateAsync(Customer customer)
    {
        const string sql = @"
            UPDATE Customers 
            SET Name = @Name,
                Email = @Email,
                Phone = @Phone,
                DateOfBirth = @DateOfBirth,
                Address = @Address,
                UpdatedAt = @UpdatedAt,
                IsActive = @IsActive
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", customer.Id);
        command.Parameters.AddWithValue("@Name", customer.Name);
        command.Parameters.AddWithValue("@Email", customer.Email);
        command.Parameters.AddWithValue("@Phone", customer.Phone);
        command.Parameters.AddWithValue("@DateOfBirth", customer.DateOfBirth == default ? (object)DBNull.Value : customer.DateOfBirth);
        command.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@IsActive", customer.IsActive);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new ArgumentException($"Customer with ID {customer.Id} not found");
        }

        customer.UpdatedAt = DateTime.UtcNow;
        return customer;
    }

    /// <summary>
    /// Deletes a customer (soft delete by setting IsActive = false)
    /// </summary>
    /// <param name="id">The customer ID to delete</param>
    /// <returns>True if deleted, false otherwise</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = @"
            UPDATE Customers 
            SET IsActive = 0, UpdatedAt = @UpdatedAt 
            WHERE Id = @Id AND IsActive = 1";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private static Customer MapFromReader(SqlDataReader reader)
    {
        return new Customer
        {
            Id = reader.GetInt32("Id"),
            Name = reader.GetString("Name"),
            Email = reader.GetString("Email"),
            Phone = reader.GetString("Phone"),
            DateOfBirth = reader.IsDBNull("DateOfBirth") ? default : reader.GetDateTime("DateOfBirth"),
            Address = reader.IsDBNull("Address") ? string.Empty : reader.GetString("Address"),
            CreatedAt = reader.GetDateTime("CreatedAt"),
            UpdatedAt = reader.GetDateTime("UpdatedAt"),
            IsActive = reader.GetBoolean("IsActive")
        };
    }
}
