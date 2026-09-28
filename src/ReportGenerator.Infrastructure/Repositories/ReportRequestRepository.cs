using Microsoft.EntityFrameworkCore;
using ReportGenerator.Application.Interfaces;
using ReportGenerator.Domain.Entities;
using ReportGenerator.Infrastructure.Persistence;

namespace ReportGenerator.Infrastructure.Repositories;

public class ReportRequestRepository : IReportRequestRepository
{
    private readonly AppDbContext _context;

    public ReportRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReportRequest> AddAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        _context.ReportRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<ReportRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ReportRequests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<ReportRequest>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ReportRequests.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        _context.ReportRequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
