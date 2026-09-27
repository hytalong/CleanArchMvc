using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Kafka;

/// <summary>
/// Skeleton adapter for Kafka producer. Implement mapping from IMessage to Kafka message
/// and use Confluent.Kafka producer in the concrete implementation.
/// </summary>
public sealed class KafkaProducerAdapter : IMessageProducer
{
    private readonly KafkaOptions _options;

    public KafkaProducerAdapter(KafkaOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<IEnumerable<PublishResult>> PublishManyAsync(IEnumerable<IMessage> messages, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement Kafka producer batch publish mapping using Confluent.Kafka.");
    }

    public Task<PublishResult> PublishAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement Kafka producer publish mapping using Confluent.Kafka.");
    }
}
