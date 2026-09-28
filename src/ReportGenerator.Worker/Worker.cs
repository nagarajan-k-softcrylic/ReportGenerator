using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ReportGenerator.Application.DTOs;
using ReportGenerator.Infrastructure.ServiceBus;
using ReportGenerator.Worker.Services;

namespace ReportGenerator.Worker;

/// <summary>
/// Consumer: listens to the "report-generation-queue" Azure Service Bus queue
/// and processes each report generation request as it arrives.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusOptions _options;
    private ServiceBusProcessor? _processor;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IOptions<ServiceBusOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _client = new ServiceBusClient(_options.ConnectionString);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor = _client.CreateProcessor(_options.QueueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;

        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Worker started listening on queue '{QueueName}'.", _options.QueueName);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // expected on shutdown
        }

        await _processor.StopProcessingAsync(CancellationToken.None);
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var message = JsonSerializer.Deserialize<ReportRequestMessage>(body);

            if (message is not null)
            {
                using var scope = _scopeFactory.CreateScope();
                var processingService = scope.ServiceProvider.GetRequiredService<ReportProcessingService>();
                await processingService.ProcessAsync(message, args.CancellationToken);
            }

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Service Bus message {MessageId}.", args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, deadLetterReason: ex.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processor error.");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.DisposeAsync();
        }
        await _client.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
