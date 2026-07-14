using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CleanArchMvc.Worker.Reconciliation.Models;

namespace CleanArchMvc.Worker.Reconciliation.Producers;

public class FileProducerService : BackgroundService
{
    private readonly ProducerConfig _producerConfig;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileProducerService> _logger;

    public FileProducerService(ProducerConfig producerConfig, IConfiguration configuration, ILogger<FileProducerService> logger)
    {
        _producerConfig = producerConfig;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var inputPath = _configuration.GetSection("Reconciliation")["InputFilePath"] ?? "./input/positions.csv";
        var topic = _configuration.GetSection("Kafka")["TopicPositions"] ?? "reconciliation.positions";

        if (!File.Exists(inputPath))
        {
            _logger.LogWarning("Input file not found: {path}", inputPath);
            return;
        }

        using var producer = new ProducerBuilder<string, string>(_producerConfig).Build();

        using var reader = new StreamReader(inputPath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        await foreach (var record in csv.GetRecordsAsync<dynamic>().WithCancellation(stoppingToken))
        {
            try
            {
                var json = JsonSerializer.Serialize(record);
                string key = record?.Id ?? Guid.NewGuid().ToString();

                await producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = json }, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error producing message from record");
            }
        }

        producer.Flush(TimeSpan.FromSeconds(30));
        _logger.LogInformation("Finished producing messages from {path}", inputPath);
    }
}
