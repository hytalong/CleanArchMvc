using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.RabbitMQ;

/// <summary>
/// Skeleton adapter for RabbitMQ producer. Implement mapping from IMessage to BasicProperties
/// and publishing to exchange with routing key.
/// </summary>
public sealed class RabbitProducerAdapter : IMessageProducer
{
    private readonly RabbitOptions _options;

    public RabbitProducerAdapter(RabbitOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task<IEnumerable<PublishResult>> PublishManyAsync(IEnumerable<IMessage> messages, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement RabbitMQ producer batch publish using RabbitMQ.Client.");
    }

    public Task<PublishResult> PublishAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement RabbitMQ producer publish using RabbitMQ.Client.");
    }
}
