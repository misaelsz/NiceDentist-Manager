using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Infrastructure.Repositories;

/// <summary>
/// SQL Server implementation of dentist repository
/// </summary>
public class DentistRepository : IDentistRepository
{
    private readonly string _connectionString;

    public DentistRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Creates a new dentist
    /// </summary>
    public async Task<Dentist> CreateAsync(Dentist dentist)
    {
        const string sql = @"
            INSERT INTO Dentists (Name, Email, Phone, LicenseNumber, Specialization, CreatedAt, UpdatedAt, IsActive, UserId)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Email, @Phone, @LicenseNumber, @Specialization, @CreatedAt, @UpdatedAt, @IsActive, @UserId)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        AddParameters(command, dentist);

        var id = (int)await command.ExecuteScalarAsync();
        dentist.Id = id;

        return dentist;
    }

    /// <summary>
    /// Gets a dentist by ID
    /// </summary>
    public async Task<Dentist?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, Name, Email, Phone, LicenseNumber, Specialization, CreatedAt, UpdatedAt, IsActive, UserId
            FROM Dentists 
            WHERE Id = @Id";

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
    /// Gets a dentist by email
    /// </summary>
    public async Task<Dentist?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT Id, Name, Email, Phone, LicenseNumber, Specialization, CreatedAt, UpdatedAt, IsActive, UserId
            FROM Dentists 
            WHERE Email = @Email";

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
    /// Gets a dentist by license number
    /// </summary>
    public async Task<Dentist?> GetByLicenseNumberAsync(string licenseNumber)
    {
        const string sql = @"
            SELECT Id, Name, Email, Phone, LicenseNumber, Specialization, CreatedAt, UpdatedAt, IsActive, UserId
            FROM Dentists 
            WHERE LicenseNumber = @LicenseNumber";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@LicenseNumber", licenseNumber);

        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    /// <summary>
    /// Gets all dentists with pagination
    /// </summary>
    public async Task<IEnumerable<Dentist>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        const string sql = @"
            SELECT Id, Name, Email, Phone, LicenseNumber, Specialization, CreatedAt, UpdatedAt, IsActive, UserId
            FROM Dentists 
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS 
            FETCH NEXT @PageSize ROWS ONLY";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        var dentists = new List<Dentist>();
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            dentists.Add(MapFromReader(reader));
        }

        return dentists;
    }

    /// <summary>
    /// Updates a dentist
    /// </summary>
    public async Task<Dentist> UpdateAsync(Dentist dentist)
    {
        const string sql = @"
            UPDATE Dentists 
            SET Name = @Name, 
                Email = @Email, 
                Phone = @Phone, 
                LicenseNumber = @LicenseNumber, 
                Specialization = @Specialization, 
                UpdatedAt = @UpdatedAt, 
                IsActive = @IsActive, 
                UserId = @UserId
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        AddParameters(command, dentist);
        command.Parameters.AddWithValue("@Id", dentist.Id);

        await command.ExecuteNonQueryAsync();

        return dentist;
    }

    /// <summary>
    /// Deletes a dentist
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Dentists WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Maps a SqlDataReader to a Dentist object
    /// </summary>
    private static Dentist MapFromReader(SqlDataReader reader)
    {
        return new Dentist
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            Phone = reader.GetString(reader.GetOrdinal("Phone")),
            LicenseNumber = reader.GetString(reader.GetOrdinal("LicenseNumber")),
            Specialization = reader.GetString(reader.GetOrdinal("Specialization")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) 
                ? (int?)null 
                : reader.GetInt32(reader.GetOrdinal("UserId"))
        };
    }

    /// <summary>
    /// Updates only the UserId field for a dentist (used by events)
    /// </summary>
    /// <param name="dentistId">The dentist ID</param>
    /// <param name="userId">The user ID to set</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> UpdateUserIdAsync(int dentistId, int userId)
    {
        const string sql = @"
            UPDATE Dentists 
            SET UserId = @UserId,
                UpdatedAt = @UpdatedAt
            WHERE Id = @DentistId";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DentistId", dentistId);
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Adds parameters to a SQL command for a dentist
    /// </summary>
    private static void AddParameters(SqlCommand command, Dentist dentist)
    {
        command.Parameters.AddWithValue("@Name", dentist.Name);
        command.Parameters.AddWithValue("@Email", dentist.Email);
        command.Parameters.AddWithValue("@Phone", dentist.Phone);
        command.Parameters.AddWithValue("@LicenseNumber", dentist.LicenseNumber);
        command.Parameters.AddWithValue("@Specialization", dentist.Specialization);
        command.Parameters.AddWithValue("@CreatedAt", dentist.CreatedAt);
        command.Parameters.AddWithValue("@UpdatedAt", dentist.UpdatedAt);
        command.Parameters.AddWithValue("@IsActive", dentist.IsActive);
        command.Parameters.AddWithValue("@UserId", (object?)dentist.UserId ?? DBNull.Value);
    }
}