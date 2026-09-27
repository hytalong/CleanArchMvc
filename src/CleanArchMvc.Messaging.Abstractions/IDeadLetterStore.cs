using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Store para mensagens mortas (DLQ). Implementação persistente recomendada.
/// </summary>
public interface IDeadLetterStore
{
    Task SaveAsync(DeadLetterMessage message, CancellationToken cancellationToken = default);
    Task<IEnumerable<DeadLetterMessage>> GetAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task RequeueAsync(Guid deadLetterId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Representa a mensagem que foi enviada para DLQ com metadados de falha.
/// </summary>
public sealed class DeadLetterMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public IMessage OriginalMessage { get; init; } = default!;
    public string Reason { get; init; } = string.Empty;
    public int AttemptCount { get; init; }
    public DateTimeOffset FailedAt { get; init; } = DateTimeOffset.UtcNow;
    public string? Origin { get; init; }
}

