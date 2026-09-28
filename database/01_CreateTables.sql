-- =============================================
-- ReportGenerator Database Schema
-- =============================================
CREATE DATABASE ReportGeneratorDb;
GO

USE ReportGeneratorDb;
GO

-- ============ ReportRequests ============
IF OBJECT_ID('dbo.ReportRequests', 'U') IS NOT NULL DROP TABLE dbo.ReportRequests;
GO
CREATE TABLE dbo.ReportRequests
(
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    ReportName      NVARCHAR(200)    NOT NULL,
    RequestedBy     NVARCHAR(100)    NOT NULL,
    RequestedDate   DATETIME         NOT NULL DEFAULT GETUTCDATE(),
    Status          NVARCHAR(50)     NOT NULL DEFAULT 'Not Processed',
    FileName        NVARCHAR(500)    NULL,
    BlobUrl         NVARCHAR(MAX)    NULL,
    FailureReason   NVARCHAR(MAX)    NULL,
    ProcessedDate   DATETIME         NULL,
    CreatedDate     DATETIME         NOT NULL DEFAULT GETUTCDATE()
);
GO

-- ============ Departments ============
IF OBJECT_ID('dbo.Departments', 'U') IS NOT NULL DROP TABLE dbo.Departments;
GO
CREATE TABLE dbo.Departments
(
    DepartmentId    INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentName  NVARCHAR(100) NOT NULL,
    Location        NVARCHAR(100) NULL
);
GO

-- ============ Employees ============
IF OBJECT_ID('dbo.Employees', 'U') IS NOT NULL DROP TABLE dbo.Employees;
GO
CREATE TABLE dbo.Employees
(
    EmployeeId      INT IDENTITY(1,1) PRIMARY KEY,
    FirstName       NVARCHAR(100) NOT NULL,
    LastName        NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(150) NULL,
    DepartmentId    INT NOT NULL FOREIGN KEY REFERENCES dbo.Departments(DepartmentId),
    DesignationName NVARCHAR(100) NULL,
    JoiningDate     DATE NOT NULL,
    IsActive        BIT NOT NULL DEFAULT 1
);
GO

-- ============ Salaries ============
IF OBJECT_ID('dbo.Salaries', 'U') IS NOT NULL DROP TABLE dbo.Salaries;
GO
CREATE TABLE dbo.Salaries
(
    SalaryId        INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId      INT NOT NULL FOREIGN KEY REFERENCES dbo.Employees(EmployeeId),
    BasicSalary     DECIMAL(18,2) NOT NULL,
    Bonus           DECIMAL(18,2) NOT NULL DEFAULT 0,
    Deductions      DECIMAL(18,2) NOT NULL DEFAULT 0,
    EffectiveDate   DATE NOT NULL
);
GO
