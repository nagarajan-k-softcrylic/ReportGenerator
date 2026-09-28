using Microsoft.EntityFrameworkCore;
using ReportGenerator.Domain.Entities;

namespace ReportGenerator.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ReportRequest> ReportRequests => Set<ReportRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportRequest>(entity =>
        {
            entity.ToTable("ReportRequests", "dbo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReportName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.RequestedBy).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.BlobUrl);
            entity.Property(e => e.FailureReason);
        });

        base.OnModelCreating(modelBuilder);
    }
}
