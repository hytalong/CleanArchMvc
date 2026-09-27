using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Abstração para consumidores. Registra handlers e controla lifecycle.
/// Implementações concretas executam tradução entre primitives do broker e MessageEnvelope.
/// </summary>
public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    void RegisterHandler<T>(IMessageHandler<T> handler, ConsumerOptions? options = null);
}

/// <summary>
/// Opções para registro do consumer/handler.
/// </summary>
public sealed class ConsumerOptions
{
    /// <summary>
    /// Nome lógico do consumer group. Usado por brokers que suportam consumer groups (Kafka).
    /// </summary>
    public string? ConsumerGroup { get; set; }

    /// <summary>
    /// Grau de paralelismo desejado para processamento de mensagens.
    /// </summary>
    public int DegreeOfParallelism { get; set; } = 1;

    /// <summary>
    /// Se verdadeiro, tenta preservar ordenação por Key quando possível.
    /// Nem todo broker/adapter suportará isto.
    /// </summary>
    public bool OrderedProcessing { get; set; } = false;
    /// <summary>
    /// Política de retry para este handler/consumer.
    /// </summary>
    public RetryPolicy? RetryPolicy { get; set; }
}

