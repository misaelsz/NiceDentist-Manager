using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NiceDentist.Manager.Application.Contracts;
using NiceDentist.Manager.Domain;

namespace NiceDentist.Manager.Infrastructure.Repositories;

/// <summary>
/// SQL Server implementation of appointment repository
/// </summary>
public class AppointmentRepository : IAppointmentRepository
{
    private readonly string _connectionString;

    public AppointmentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found");
    }

    /// <summary>
    /// Creates a new appointment
    /// </summary>
    /// <param name="appointment">The appointment to create</param>
    /// <returns>The created appointment with assigned ID</returns>
    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        const string sql = @"
            INSERT INTO Appointments (CustomerId, DentistId, AppointmentDateTime, ProcedureType, Notes, Status, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.Id
            VALUES (@CustomerId, @DentistId, @AppointmentDateTime, @ProcedureType, @Notes, @Status, @CreatedAt, @UpdatedAt)";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
        command.Parameters.AddWithValue("@DentistId", appointment.DentistId);
        command.Parameters.AddWithValue("@AppointmentDateTime", appointment.AppointmentDateTime);
        command.Parameters.AddWithValue("@ProcedureType", appointment.ProcedureType);
        command.Parameters.AddWithValue("@Notes", appointment.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Status", appointment.Status.ToString());
        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        var id = await command.ExecuteScalarAsync();
        appointment.Id = Convert.ToInt32(id);
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        return appointment;
    }

    /// <summary>
    /// Gets an appointment by ID
    /// </summary>
    /// <param name="id">The appointment ID</param>
    /// <returns>The appointment if found, null otherwise</returns>
    public async Task<Appointment?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT Id, CustomerId, DentistId, AppointmentDateTime, ProcedureType, Notes, Status, CreatedAt, UpdatedAt
            FROM Appointments 
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
    /// Gets all appointments for a specific customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <returns>List of appointments for the customer</returns>
    public async Task<IEnumerable<Appointment>> GetByCustomerIdAsync(int customerId)
    {
        const string sql = @"
            SELECT Id, CustomerId, DentistId, AppointmentDateTime, ProcedureType, Notes, Status, CreatedAt, UpdatedAt
            FROM Appointments 
            WHERE CustomerId = @CustomerId
            ORDER BY AppointmentDateTime";

        return await ExecuteQueryAsync(sql, new { CustomerId = customerId });
    }

    /// <summary>
    /// Gets all appointments for a specific dentist
    /// </summary>
    /// <param name="dentistId">The dentist ID</param>
    /// <returns>List of appointments for the dentist</returns>
    public async Task<IEnumerable<Appointment>> GetByDentistIdAsync(int dentistId)
    {
        const string sql = @"
            SELECT Id, CustomerId, DentistId, AppointmentDateTime, ProcedureType, Notes, Status, CreatedAt, UpdatedAt
            FROM Appointments 
            WHERE DentistId = @DentistId
            ORDER BY AppointmentDateTime";

        return await ExecuteQueryAsync(sql, new { DentistId = dentistId });
    }

    /// <summary>
    /// Gets appointments by date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of appointments in the date range</returns>
    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT Id, CustomerId, DentistId, AppointmentDateTime, ProcedureType, Notes, Status, CreatedAt, UpdatedAt
            FROM Appointments 
            WHERE AppointmentDateTime >= @StartDate AND AppointmentDateTime <= @EndDate
            ORDER BY AppointmentDateTime";

        return await ExecuteQueryAsync(sql, new { StartDate = startDate, EndDate = endDate });
    }

    /// <summary>
    /// Checks if a customer has conflicting appointments
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="excludeAppointmentId">Appointment ID to exclude from check (for updates)</param>
    /// <returns>True if there's a conflict, false otherwise</returns>
    public async Task<bool> HasCustomerConflictAsync(int customerId, DateTime appointmentDateTime, int? excludeAppointmentId = null)
    {
        var sql = @"
            SELECT COUNT(1) 
            FROM Appointments 
            WHERE CustomerId = @CustomerId 
            AND AppointmentDateTime = @AppointmentDateTime 
            AND Status != @CancelledStatus";

        if (excludeAppointmentId.HasValue)
        {
            sql += " AND Id != @ExcludeId";
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@CustomerId", customerId);
        command.Parameters.AddWithValue("@AppointmentDateTime", appointmentDateTime);
        command.Parameters.AddWithValue("@CancelledStatus", (int)AppointmentStatus.Cancelled);
        
        if (excludeAppointmentId.HasValue)
        {
            command.Parameters.AddWithValue("@ExcludeId", excludeAppointmentId.Value);
        }

        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count) > 0;
    }

    /// <summary>
    /// Checks if a dentist has conflicting appointments
    /// </summary>
    /// <param name="dentistId">Dentist ID</param>
    /// <param name="appointmentDateTime">Appointment date and time</param>
    /// <param name="excludeAppointmentId">Appointment ID to exclude from check (for updates)</param>
    /// <returns>True if there's a conflict, false otherwise</returns>
    public async Task<bool> HasDentistConflictAsync(int dentistId, DateTime appointmentDateTime, int? excludeAppointmentId = null)
    {
        var sql = @"
            SELECT COUNT(1) 
            FROM Appointments 
            WHERE DentistId = @DentistId 
            AND AppointmentDateTime = @AppointmentDateTime 
            AND Status != @CancelledStatus";

        if (excludeAppointmentId.HasValue)
        {
            sql += " AND Id != @ExcludeId";
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@DentistId", dentistId);
        command.Parameters.AddWithValue("@AppointmentDateTime", appointmentDateTime);
        command.Parameters.AddWithValue("@CancelledStatus", (int)AppointmentStatus.Cancelled);
        
        if (excludeAppointmentId.HasValue)
        {
            command.Parameters.AddWithValue("@ExcludeId", excludeAppointmentId.Value);
        }

        var count = await command.ExecuteScalarAsync();
        return Convert.ToInt32(count) > 0;
    }

    /// <summary>
    /// Updates an appointment
    /// </summary>
    /// <param name="appointment">The appointment to update</param>
    /// <returns>The updated appointment</returns>
    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        const string sql = @"
            UPDATE Appointments 
            SET CustomerId = @CustomerId,
                DentistId = @DentistId,
                AppointmentDateTime = @AppointmentDateTime,
                ProcedureType = @ProcedureType,
                Notes = @Notes,
                Status = @Status,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", appointment.Id);
        command.Parameters.AddWithValue("@CustomerId", appointment.CustomerId);
        command.Parameters.AddWithValue("@DentistId", appointment.DentistId);
        command.Parameters.AddWithValue("@AppointmentDateTime", appointment.AppointmentDateTime);
        command.Parameters.AddWithValue("@ProcedureType", appointment.ProcedureType);
        command.Parameters.AddWithValue("@Notes", appointment.Notes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Status", appointment.Status.ToString());
        command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        if (rowsAffected == 0)
        {
            throw new ArgumentException($"Appointment with ID {appointment.Id} not found");
        }

        appointment.UpdatedAt = DateTime.UtcNow;
        return appointment;
    }

    /// <summary>
    /// Deletes an appointment
    /// </summary>
    /// <param name="id">The appointment ID to delete</param>
    /// <returns>True if deleted, false otherwise</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Appointments WHERE Id = @Id";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Gets all appointments with filtering and pagination
    /// </summary>
    public async Task<IEnumerable<Appointment>> GetAllAsync(
        int page = 1, 
        int pageSize = 10, 
        int? customerId = null, 
        int? dentistId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        AppointmentStatus? status = null)
    {
        var sql = @"
            SELECT Id, CustomerId, DentistId, AppointmentDateTime, ProcedureType, Notes, Status, CreatedAt, UpdatedAt
            FROM Appointments 
            WHERE 1=1";

        var parameters = new List<SqlParameter>();

        if (customerId.HasValue)
        {
            sql += " AND CustomerId = @CustomerId";
            parameters.Add(new SqlParameter("@CustomerId", customerId.Value));
        }

        if (dentistId.HasValue)
        {
            sql += " AND DentistId = @DentistId";
            parameters.Add(new SqlParameter("@DentistId", dentistId.Value));
        }

        if (startDate.HasValue)
        {
            sql += " AND AppointmentDateTime >= @StartDate";
            parameters.Add(new SqlParameter("@StartDate", startDate.Value));
        }

        if (endDate.HasValue)
        {
            sql += " AND AppointmentDateTime <= @EndDate";
            parameters.Add(new SqlParameter("@EndDate", endDate.Value));
        }

        if (status.HasValue)
        {
            sql += " AND Status = @Status";
            parameters.Add(new SqlParameter("@Status", (int)status.Value));
        }

        sql += @"
            ORDER BY AppointmentDateTime
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add(new SqlParameter("@Offset", (page - 1) * pageSize));
        parameters.Add(new SqlParameter("@PageSize", pageSize));

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());

        var appointments = new List<Appointment>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            appointments.Add(MapFromReader(reader));
        }

        return appointments;
    }

    private async Task<IEnumerable<Appointment>> ExecuteQueryAsync(string sql, object? parameters = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        
        if (parameters != null)
        {
            foreach (var prop in parameters.GetType().GetProperties())
            {
                command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(parameters) ?? DBNull.Value);
            }
        }

        var appointments = new List<Appointment>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            appointments.Add(MapFromReader(reader));
        }

        return appointments;
    }

    private static Appointment MapFromReader(SqlDataReader reader)
    {
        return new Appointment
        {
            Id = reader.GetInt32("Id"),
            CustomerId = reader.GetInt32("CustomerId"),
            DentistId = reader.GetInt32("DentistId"),
            AppointmentDateTime = reader.GetDateTime("AppointmentDateTime"),
            ProcedureType = reader.GetString("ProcedureType"),
            Notes = reader.IsDBNull("Notes") ? string.Empty : reader.GetString("Notes"),
            Status = Enum.Parse<AppointmentStatus>(reader.GetString("Status")),
            CreatedAt = reader.GetDateTime("CreatedAt"),
            UpdatedAt = reader.GetDateTime("UpdatedAt")
        };
    }
}
