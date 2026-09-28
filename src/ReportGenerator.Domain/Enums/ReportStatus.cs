namespace ReportGenerator.Domain.Enums;

/// <summary>
/// Status values for a report request. Stored as string in the database.
/// </summary>
public static class ReportStatus
{
    public const string NotProcessed = "Not Processed";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
}
