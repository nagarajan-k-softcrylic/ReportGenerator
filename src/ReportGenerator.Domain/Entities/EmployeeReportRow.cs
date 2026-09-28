namespace ReportGenerator.Domain.Entities;

/// <summary>
/// Flat projection returned by dbo.usp_GenerateEmployeeReport.
/// Used to build the Excel report.
/// </summary>
public class EmployeeReportRow
{
    public int EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? DesignationName { get; set; }
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; }
    public decimal? BasicSalary { get; set; }
    public decimal? Bonus { get; set; }
    public decimal? Deductions { get; set; }
    public decimal? NetSalary { get; set; }
}
