using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ReportGenerator.Application.DTOs;
using ReportGenerator.Application.Interfaces;

namespace ReportGenerator.Infrastructure.ServiceBus;

/// <summary>
/// Producer: publishes report generation requests onto the Azure Service Bus queue.
/// </summary>
public class ServiceBusPublisher : IServiceBusPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusPublisher(IOptions<ServiceBusOptions> options)
    {
        var settings = options.Value;
        _client = new ServiceBusClient(settings.ConnectionString);
        _sender = _client.CreateSender(settings.QueueName);
    }

    public async Task PublishReportRequestAsync(ReportRequestMessage message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = "ReportGenerationRequest"
        };

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
