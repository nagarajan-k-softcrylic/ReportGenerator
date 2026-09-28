using ClosedXML.Excel;
using ReportGenerator.Domain.Entities;

namespace ReportGenerator.Worker.Services;

/// <summary>
/// Builds the employee report Excel workbook using ClosedXML.
/// </summary>
public class ExcelReportGenerator
{
    public MemoryStream Generate(List<EmployeeReportRow> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Employee Report");

        var headers = new[]
        {
            "Employee Id", "First Name", "Last Name", "Email", "Department",
            "Location", "Designation", "Joining Date", "Active",
            "Basic Salary", "Bonus", "Deductions", "Net Salary"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        var rowIndex = 2;
        foreach (var row in rows)
        {
            sheet.Cell(rowIndex, 1).Value = row.EmployeeId;
            sheet.Cell(rowIndex, 2).Value = row.FirstName;
            sheet.Cell(rowIndex, 3).Value = row.LastName;
            sheet.Cell(rowIndex, 4).Value = row.Email;
            sheet.Cell(rowIndex, 5).Value = row.DepartmentName;
            sheet.Cell(rowIndex, 6).Value = row.Location;
            sheet.Cell(rowIndex, 7).Value = row.DesignationName;
            sheet.Cell(rowIndex, 8).Value = row.JoiningDate;
            sheet.Cell(rowIndex, 9).Value = row.IsActive ? "Yes" : "No";
            sheet.Cell(rowIndex, 10).Value = row.BasicSalary ?? 0;
            sheet.Cell(rowIndex, 11).Value = row.Bonus ?? 0;
            sheet.Cell(rowIndex, 12).Value = row.Deductions ?? 0;
            sheet.Cell(rowIndex, 13).Value = row.NetSalary ?? 0;
            rowIndex++;
        }

        sheet.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
