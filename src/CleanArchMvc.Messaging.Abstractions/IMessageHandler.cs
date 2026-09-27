using System.Threading;
using System.Threading.Tasks;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Handler de mensagens fortemente tipado. A implementação deve conter lógica idempotente
/// ou usar IIdempotencyStore antes de aplicar efeitos colaterais.
/// </summary>
public interface IMessageHandler<T>
{
    Task HandleAsync(MessageEnvelope<T> envelope, HandlerContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// Contexto fornecido ao handler com utilitários para ack/retry/reject.
/// Implementações concretas do consumer proverão comportamento real desses métodos.
/// </summary>
public sealed class HandlerContext
{
    /// <summary>
    /// Id lógico do handler (pode ser usado no idempotency/store).
    /// </summary>
    public string? HandlerId { get; init; }

    /// <summary>
    /// Tentativa atual (1 = primeira tentativa).
    /// </summary>
    public int Attempt { get; set; }

    /// <summary>
    /// Acknowledge/commit da mensagem (no broker) — implementação concreta deve executar.
    /// </summary>
    public Task AcknowledgeAsync() => Task.CompletedTask;

    /// <summary>
    /// Rejeita a mensagem (não reconfirma). Semântica depende do broker/adapter.
    /// </summary>
    public Task RejectAsync() => Task.CompletedTask;

    /// <summary>
    /// Solicita re-enfileiramento com delay. Implementações que não suportam delay podem
    /// simular com header RetryCount e re-publicação em tópico/queue de retry.
    /// </summary>
    public Task RetryLaterAsync(System.TimeSpan delay) => Task.CompletedTask;
}

