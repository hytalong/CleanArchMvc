using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CleanArchMvc.Worker.Reconciliation.Consumers;

public class ReconciliationConsumerService : BackgroundService
{
    private readonly ConsumerConfig _consumerConfig;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReconciliationConsumerService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ReconciliationConsumerService(ConsumerConfig consumerConfig, IConfiguration configuration, ILogger<ReconciliationConsumerService> logger, IServiceScopeFactory scopeFactory)
    {
        _consumerConfig = consumerConfig;
        _configuration = configuration;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            var topic = _configuration.GetSection("Kafka")["TopicPositions"] ?? "reconciliation.positions";
            var resultsFolder = _configuration.GetSection("Reconciliation")["ResultsFolder"] ?? "./ReconciliationResults";
            Directory.CreateDirectory(resultsFolder);

            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            consumer.Subscribe(topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = consumer.Consume(stoppingToken);
                        // processamento simples: salvar payload em arquivo NDJSON
                        var fileName = Path.Combine(resultsFolder, $"{DateTime.UtcNow:yyyyMMdd}.ndjson");
                        File.AppendAllText(fileName, cr.Message.Value + Environment.NewLine);

                        // commit manual após persistência
                        consumer.Commit(cr);
                    }
                    catch (ConsumeException cex)
                    {
                        _logger.LogError(cex, "Consume error");
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error processing message");
                    }
                }
            }
            finally
            {
                try { consumer.Close(); } catch { }
            }
        }, stoppingToken);
    }
}
