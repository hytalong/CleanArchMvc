using System;

namespace CleanArchMvc.Messaging.Kafka;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ClientId { get; set; } = "cleanarchmvc";
    public int KafkaMaxRetries { get; set; } = 3;
    public TimeSpan RetryBackoff { get; set; } = TimeSpan.FromSeconds(2);
    // Mais opções específicas podem ser adicionadas conforme necessário
}
