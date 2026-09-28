using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ReportGenerator.Application.Interfaces;
using ReportGenerator.Domain.Entities;

namespace ReportGenerator.Infrastructure.Repositories;

/// <summary>
/// Executes dbo.usp_GenerateEmployeeReport using ADO.NET (raw SQL execution
/// for stored procedure result sets, independent of EF Core's DbContext model).
/// </summary>
public class EmployeeReportRepository : IEmployeeReportRepository
{
    private readonly string _connectionString;

    public EmployeeReportRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");
    }

    public async Task<List<EmployeeReportRow>> GetEmployeeReportAsync(bool isActiveOnly = true, CancellationToken cancellationToken = default)
    {
        var results = new List<EmployeeReportRow>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("dbo.usp_GenerateEmployeeReport", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@IsActiveOnly", isActiveOnly);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new EmployeeReportRow
            {
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                Location = reader.IsDBNull(reader.GetOrdinal("Location")) ? null : reader.GetString(reader.GetOrdinal("Location")),
                DesignationName = reader.IsDBNull(reader.GetOrdinal("DesignationName")) ? null : reader.GetString(reader.GetOrdinal("DesignationName")),
                JoiningDate = reader.GetDateTime(reader.GetOrdinal("JoiningDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                BasicSalary = reader.IsDBNull(reader.GetOrdinal("BasicSalary")) ? null : reader.GetDecimal(reader.GetOrdinal("BasicSalary")),
                Bonus = reader.IsDBNull(reader.GetOrdinal("Bonus")) ? null : reader.GetDecimal(reader.GetOrdinal("Bonus")),
                Deductions = reader.IsDBNull(reader.GetOrdinal("Deductions")) ? null : reader.GetDecimal(reader.GetOrdinal("Deductions")),
                NetSalary = reader.IsDBNull(reader.GetOrdinal("NetSalary")) ? null : reader.GetDecimal(reader.GetOrdinal("NetSalary"))
            });
        }

        return results;
    }
}
