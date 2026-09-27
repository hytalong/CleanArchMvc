using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Kafka;

/// <summary>
/// Skeleton adapter for Kafka consumer. Implement mapping from Kafka consumer records
/// to MessageEnvelope and manage offsets, consumer group, rebalancing, etc.
/// </summary>
public sealed class KafkaConsumerAdapter : IMessageConsumer
{
    private readonly KafkaOptions _options;

    public KafkaConsumerAdapter(KafkaOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement Kafka consumer start using Confluent.Kafka consumer and manage handlers/offsets.");
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement Kafka consumer stop and graceful shutdown.");
    }

    public void RegisterHandler<T>(IMessageHandler<T> handler, ConsumerOptions? options = null)
    {
        throw new NotImplementedException("Implement handler registration and dispatch for Kafka consumer.");
    }
}
