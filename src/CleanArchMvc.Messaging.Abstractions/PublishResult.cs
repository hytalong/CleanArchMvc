using System;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Resultado genérico de uma tentativa de publicação.
/// </summary>
public sealed class PublishResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public BrokerMetadata? BrokerMetadata { get; init; }
}

/// <summary>
/// Metadados específicos do broker que podem ser úteis para observability e debugging.
/// Ex.: topic/partition/offset (Kafka) ou exchange/routingKey (RabbitMQ).
/// </summary>
public sealed class BrokerMetadata
{
    // Campos genéricos; implementações concretas podem popular conforme necessário
    public string? TopicOrExchange { get; init; }
    public int? Partition { get; init; }
    public long? Offset { get; init; }
    public string? BrokerSpecificId { get; init; }
}

