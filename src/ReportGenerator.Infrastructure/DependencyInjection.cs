using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReportGenerator.Application.Interfaces;
using ReportGenerator.Infrastructure.Persistence;
using ReportGenerator.Infrastructure.Repositories;
using ReportGenerator.Infrastructure.ServiceBus;
using ReportGenerator.Infrastructure.Storage;

namespace ReportGenerator.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence, Service Bus and Blob Storage services.
    /// Shared between the API and the Worker host.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<ServiceBusOptions>(configuration.GetSection(ServiceBusOptions.SectionName));
        services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));

        services.AddScoped<IReportRequestRepository, ReportRequestRepository>();
        services.AddScoped<IEmployeeReportRepository, EmployeeReportRepository>();
        services.AddSingleton<IServiceBusPublisher, ServiceBusPublisher>();
        services.AddSingleton<IBlobStorageService, BlobStorageService>();

        return services;
    }
}
