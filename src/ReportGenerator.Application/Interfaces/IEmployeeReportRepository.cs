using ReportGenerator.Domain.Entities;

namespace ReportGenerator.Application.Interfaces;

/// <summary>
/// Executes the dbo.usp_GenerateEmployeeReport stored procedure.
/// Implemented in Infrastructure and consumed by the Worker.
/// </summary>
public interface IEmployeeReportRepository
{
    Task<List<EmployeeReportRow>> GetEmployeeReportAsync(
        bool isActiveOnly = true,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}
