-- =============================================
-- Sample Data: Departments, Employees, Salaries
-- =============================================
USE ReportGeneratorDb;
GO

INSERT INTO dbo.Departments (DepartmentName, Location) VALUES
('Engineering', 'Bangalore'),
('Human Resources', 'Mumbai'),
('Finance', 'Pune'),
('Sales', 'Delhi'),
('Marketing', 'Hyderabad'),
('IT Support', 'Chennai');
GO

-- Generate 60 sample employees using a numbers sequence
;WITH Numbers AS
(
    SELECT TOP (60) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
    FROM sys.all_objects
)
INSERT INTO dbo.Employees (FirstName, LastName, Email, DepartmentId, DesignationName, JoiningDate, IsActive)
SELECT
    CONCAT('FirstName', N),
    CONCAT('LastName', N),
    CONCAT('employee', N, '@company.com'),
    ((N - 1) % 6) + 1,
    CASE (N % 5)
        WHEN 0 THEN 'Manager'
        WHEN 1 THEN 'Senior Engineer'
        WHEN 2 THEN 'Engineer'
        WHEN 3 THEN 'Analyst'
        ELSE 'Associate'
    END,
    DATEADD(DAY, -(N * 37), CAST(GETDATE() AS DATE)),
    CASE WHEN N % 10 = 0 THEN 0 ELSE 1 END
FROM Numbers;
GO

-- Generate matching salary records
INSERT INTO dbo.Salaries (EmployeeId, BasicSalary, Bonus, Deductions, EffectiveDate)
SELECT
    EmployeeId,
    30000 + (EmployeeId * 500),
    2000 + (EmployeeId % 5) * 500,
    500 + (EmployeeId % 3) * 200,
    DATEADD(MONTH, -1, CAST(GETDATE() AS DATE))
FROM dbo.Employees;
GO
