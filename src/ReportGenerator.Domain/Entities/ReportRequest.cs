namespace ReportGenerator.Domain.Entities;

/// <summary>
/// Represents a single report generation request tracked end-to-end
/// from submission through background processing to completion/failure.
/// </summary>
public class ReportRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ReportName { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = Domain.Enums.ReportStatus.NotProcessed;
    public string? FileName { get; set; }
    public string? BlobUrl { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
