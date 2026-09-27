using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Repositório para Outbox (mensagens armazenadas localmente antes de serem publicadas).
/// Usado para garantir atomicidade entre operações do banco de dados e envio de mensagens.
/// </summary>
public interface IOutboxRepository
{
    Task SaveOutgoingAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IEnumerable<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkSentAsync(Guid outboxMessageId, BrokerMetadata brokerMetadata, CancellationToken cancellationToken = default);
}

/// <summary>
/// Modelo de registro no Outbox.
/// Contém a mensagem a ser enviada e metadados de criação.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public IMessage Message { get; init; } = default!;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
