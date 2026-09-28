namespace ReportGenerator.Application.DTOs;

public class ReportRequestDto
{
    public Guid Id { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
    public string? BlobUrl { get; set; }
    public DateTime? ProcessedDate { get; set; }
}

public class ReportRequestMessage
{
    public Guid RequestId { get; set; }
    public string ReportName { get; set; } = string.Empty;
}
