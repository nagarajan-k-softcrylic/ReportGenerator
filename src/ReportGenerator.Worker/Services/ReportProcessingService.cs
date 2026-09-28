using ReportGenerator.Application.DTOs;
using ReportGenerator.Application.Interfaces;
using ReportGenerator.Domain.Enums;

namespace ReportGenerator.Worker.Services;

/// <summary>
/// Orchestrates the end-to-end processing of a single report request message:
/// set In Progress -> run stored procedure -> build Excel -> upload to Blob -> set Completed/Failed.
/// </summary>
public class ReportProcessingService
{
    private readonly IReportRequestRepository _reportRequestRepository;
    private readonly IEmployeeReportRepository _employeeReportRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ExcelReportGenerator _excelReportGenerator;
    private readonly ILogger<ReportProcessingService> _logger;

    public ReportProcessingService(
        IReportRequestRepository reportRequestRepository,
        IEmployeeReportRepository employeeReportRepository,
        IBlobStorageService blobStorageService,
        ExcelReportGenerator excelReportGenerator,
        ILogger<ReportProcessingService> logger)
    {
        _reportRequestRepository = reportRequestRepository;
        _employeeReportRepository = employeeReportRepository;
        _blobStorageService = blobStorageService;
        _excelReportGenerator = excelReportGenerator;
        _logger = logger;
    }

    public async Task ProcessAsync(ReportRequestMessage message, CancellationToken cancellationToken)
    {
        var reportRequest = await _reportRequestRepository.GetByIdAsync(message.RequestId, cancellationToken);
        if (reportRequest is null)
        {
            _logger.LogWarning("ReportRequest {RequestId} not found. Skipping.", message.RequestId);
            return;
        }

        try
        {
            reportRequest.Status = ReportStatus.InProgress;
            await _reportRequestRepository.UpdateAsync(reportRequest, cancellationToken);

            var rows = await _employeeReportRepository.GetEmployeeReportAsync(isActiveOnly: true, cancellationToken);

            using var excelStream = _excelReportGenerator.Generate(rows);

            var fileName = $"{reportRequest.ReportName.Replace(' ', '_')}_{reportRequest.Id}.xlsx";
            var blobUrl = await _blobStorageService.UploadAsync(fileName, excelStream, cancellationToken);

            reportRequest.Status = ReportStatus.Completed;
            reportRequest.FileName = fileName;
            reportRequest.BlobUrl = blobUrl;
            reportRequest.ProcessedDate = DateTime.UtcNow;
            reportRequest.FailureReason = null;

            await _reportRequestRepository.UpdateAsync(reportRequest, cancellationToken);

            _logger.LogInformation("Report {RequestId} completed successfully.", reportRequest.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Report {RequestId} failed.", message.RequestId);

            reportRequest.Status = ReportStatus.Failed;
            reportRequest.FailureReason = ex.Message;
            reportRequest.ProcessedDate = DateTime.UtcNow;

            await _reportRequestRepository.UpdateAsync(reportRequest, cancellationToken);
        }
    }
}
