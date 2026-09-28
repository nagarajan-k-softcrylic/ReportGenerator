-- =============================================
-- Migration: Add StartDate / EndDate to ReportRequests
-- Run this if dbo.ReportRequests already exists without these columns.
-- =============================================
USE ReportGeneratorDb;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ReportRequests') AND name = 'StartDate'
)
BEGIN
    ALTER TABLE dbo.ReportRequests ADD StartDate DATE NULL;
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ReportRequests') AND name = 'EndDate'
)
BEGIN
    ALTER TABLE dbo.ReportRequests ADD EndDate DATE NULL;
END
GO
