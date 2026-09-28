-- =============================================
-- Stored Procedure: usp_GenerateEmployeeReport
-- Purpose: Returns complete employee report data
--          joining Employees, Departments and latest Salaries
-- =============================================
USE ReportGeneratorDb;
GO

IF OBJECT_ID('dbo.usp_GenerateEmployeeReport', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GenerateEmployeeReport;
GO

CREATE PROCEDURE dbo.usp_GenerateEmployeeReport
    @IsActiveOnly BIT = 1,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH LatestSalary AS
    (
        SELECT
            s.EmployeeId,
            s.BasicSalary,
            s.Bonus,
            s.Deductions,
            s.EffectiveDate,
            ROW_NUMBER() OVER (PARTITION BY s.EmployeeId ORDER BY s.EffectiveDate DESC) AS rn
        FROM dbo.Salaries s
    )
    SELECT
        e.EmployeeId,
        e.FirstName,
        e.LastName,
        e.Email,
        d.DepartmentName,
        d.Location,
        e.DesignationName,
        e.JoiningDate,
        e.IsActive,
        ls.BasicSalary,
        ls.Bonus,
        ls.Deductions,
        (ISNULL(ls.BasicSalary, 0) + ISNULL(ls.Bonus, 0) - ISNULL(ls.Deductions, 0)) AS NetSalary
    FROM dbo.Employees e
    INNER JOIN dbo.Departments d ON d.DepartmentId = e.DepartmentId
    LEFT JOIN LatestSalary ls ON ls.EmployeeId = e.EmployeeId AND ls.rn = 1
    WHERE (@IsActiveOnly = 0 OR e.IsActive = 1)
      AND (@StartDate IS NULL OR e.JoiningDate >= @StartDate)
      AND (@EndDate IS NULL OR e.JoiningDate <= @EndDate)
    ORDER BY d.DepartmentName ASC, e.LastName ASC, e.FirstName ASC;
END
GO
