using MediatR;
using ReportGenerator.Application.DTOs;
using ReportGenerator.Application.Interfaces;

namespace ReportGenerator.Application.Reports.Queries;

public record GetAllReportRequestsQuery : IRequest<List<ReportRequestDto>>;

public class GetAllReportRequestsQueryHandler : IRequestHandler<GetAllReportRequestsQuery, List<ReportRequestDto>>
{
    private readonly IReportRequestRepository _repository;

    public GetAllReportRequestsQueryHandler(IReportRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ReportRequestDto>> Handle(GetAllReportRequestsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return items
            .OrderByDescending(r => r.RequestedDate)
            .Select(r => new ReportRequestDto
            {
                Id = r.Id,
                ReportName = r.ReportName,
                RequestedBy = r.RequestedBy,
                RequestedDate = r.RequestedDate,
                Status = r.Status,
                FailureReason = r.FailureReason,
                BlobUrl = r.BlobUrl,
                ProcessedDate = r.ProcessedDate
            }).ToList();
    }
}
