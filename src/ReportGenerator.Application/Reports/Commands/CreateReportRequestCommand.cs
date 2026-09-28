using MediatR;
using ReportGenerator.Application.DTOs;
using ReportGenerator.Application.Interfaces;
using ReportGenerator.Domain.Entities;
using ReportGenerator.Domain.Enums;

namespace ReportGenerator.Application.Reports.Commands;

public record CreateReportRequestCommand(
    string ReportName,
    string RequestedBy,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<ReportRequestDto>;

public class CreateReportRequestCommandHandler : IRequestHandler<CreateReportRequestCommand, ReportRequestDto>
{
    private readonly IReportRequestRepository _repository;
    private readonly IServiceBusPublisher _publisher;

    public CreateReportRequestCommandHandler(IReportRequestRepository repository, IServiceBusPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<ReportRequestDto> Handle(CreateReportRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = new ReportRequest
        {
            Id = Guid.NewGuid(),
            ReportName = request.ReportName,
            RequestedBy = request.RequestedBy,
            RequestedDate = DateTime.UtcNow,
            Status = ReportStatus.NotProcessed,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedDate = DateTime.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);

        await _publisher.PublishReportRequestAsync(
            new ReportRequestMessage
            {
                RequestId = entity.Id,
                ReportName = entity.ReportName,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate
            },
            cancellationToken);

        return new ReportRequestDto
        {
            Id = entity.Id,
            ReportName = entity.ReportName,
            RequestedBy = entity.RequestedBy,
            RequestedDate = entity.RequestedDate,
            Status = entity.Status,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate
        };
    }
}
