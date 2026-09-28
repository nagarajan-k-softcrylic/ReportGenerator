using MediatR;
using Microsoft.AspNetCore.Mvc;
using ReportGenerator.Application.Reports.Commands;
using ReportGenerator.Application.Reports.Queries;

namespace ReportGenerator.API.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public record CreateReportRequestBody(string ReportName, string RequestedBy, DateTime? StartDate = null, DateTime? EndDate = null);

    // POST /api/reports/request
    [HttpPost("request")]
    public async Task<IActionResult> CreateRequest([FromBody] CreateReportRequestBody body, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.ReportName) || string.IsNullOrWhiteSpace(body.RequestedBy))
        {
            return BadRequest("ReportName and RequestedBy are required.");
        }

        if (body.StartDate.HasValue && body.EndDate.HasValue && body.StartDate > body.EndDate)
        {
            return BadRequest("StartDate must be earlier than or equal to EndDate.");
        }

        var result = await _mediator.Send(
            new CreateReportRequestCommand(body.ReportName, body.RequestedBy, body.StartDate, body.EndDate),
            cancellationToken);
        return Ok(result);
    }

    // GET /api/reports
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllReportRequestsQuery(), cancellationToken);
        return Ok(result);
    }

    // GET /api/reports/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportRequestByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    // GET /api/reports/download/{id}
    [HttpGet("download/{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportRequestByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        if (string.IsNullOrEmpty(result.BlobUrl)) return NotFound("Report is not yet available for download.");

        return Redirect(result.BlobUrl);
    }
}
