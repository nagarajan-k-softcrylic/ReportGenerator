using MediatR;
using ReportGenerator.Application.DTOs;
using ReportGenerator.Application.Interfaces;

namespace ReportGenerator.Application.Reports.Queries;

public record GetReportRequestByIdQuery(Guid Id) : IRequest<ReportRequestDto?>;

public class GetReportRequestByIdQueryHandler : IRequestHandler<GetReportRequestByIdQuery, ReportRequestDto?>
{
    private readonly IReportRequestRepository _repository;

    public GetReportRequestByIdQueryHandler(IReportRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ReportRequestDto?> Handle(GetReportRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (r is null) return null;

        return new ReportRequestDto
        {
            Id = r.Id,
            ReportName = r.ReportName,
            RequestedBy = r.RequestedBy,
            RequestedDate = r.RequestedDate,
            Status = r.Status,
            FailureReason = r.FailureReason,
            BlobUrl = r.BlobUrl,
            ProcessedDate = r.ProcessedDate
        };
    }
}
