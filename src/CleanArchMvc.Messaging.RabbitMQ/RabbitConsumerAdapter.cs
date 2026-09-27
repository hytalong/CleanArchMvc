using System;
using System.Threading;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.RabbitMQ;

/// <summary>
/// Skeleton adapter for RabbitMQ consumer. Implement mapping from deliveries to MessageEnvelope
/// and manage acknowledgements, DLX, and per-queue consumers.
/// </summary>
public sealed class RabbitConsumerAdapter : IMessageConsumer
{
    private readonly RabbitOptions _options;

    public RabbitConsumerAdapter(RabbitOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement RabbitMQ consumer start using RabbitMQ.Client and manage handlers/ack.");
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement RabbitMQ consumer stop and graceful shutdown.");
    }

    public void RegisterHandler<T>(IMessageHandler<T> handler, ConsumerOptions? options = null)
    {
        throw new NotImplementedException("Implement handler registration and dispatch for RabbitMQ consumer.");
    }
}
