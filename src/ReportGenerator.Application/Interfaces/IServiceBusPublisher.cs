using ReportGenerator.Application.DTOs;

namespace ReportGenerator.Application.Interfaces;

public interface IServiceBusPublisher
{
    Task PublishReportRequestAsync(ReportRequestMessage message, CancellationToken cancellationToken = default);
}
