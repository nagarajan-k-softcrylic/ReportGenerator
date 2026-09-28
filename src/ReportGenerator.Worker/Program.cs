using ReportGenerator.Infrastructure;
using ReportGenerator.Worker;
using ReportGenerator.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ReportProcessingService>();
builder.Services.AddSingleton<ExcelReportGenerator>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
