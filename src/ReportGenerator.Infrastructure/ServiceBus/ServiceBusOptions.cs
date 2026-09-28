namespace ReportGenerator.Infrastructure.ServiceBus;

public class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";

    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = "report-generation-queue";
}
