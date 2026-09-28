using ReportGenerator.Domain.Entities;

namespace ReportGenerator.Application.Interfaces;

public interface IReportRequestRepository
{
    Task<ReportRequest> AddAsync(ReportRequest request, CancellationToken cancellationToken = default);
    Task<ReportRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ReportRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(ReportRequest request, CancellationToken cancellationToken = default);
}
