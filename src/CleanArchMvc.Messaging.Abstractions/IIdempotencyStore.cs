using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Store para controle de idempotência. Deve ser persistente (DB) e garantir atomicidade
/// em TryMarkProcessedAsync (p.ex. INSERT com unique constraint em MessageId).
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>
    /// Tenta marcar a mensagem como processada.
    /// Retorna true se esta foi a primeira vez (deve processar), false se já processada.
    /// A operação deve ser atômica para evitar races entre consumidores concorrentes.
    /// </summary>
    Task<bool> TryMarkProcessedAsync(Guid messageId, string handlerId, DateTimeOffset processedAt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpa entradas antigas (janela de retenção configurável).
    /// </summary>
    Task CleanupOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove marcação de processamento (por exemplo, quando ocorreu falha e a mensagem deve ser reprocessada).
    /// </summary>
    Task RemoveAsync(Guid messageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Extensões esperadas em algumas implementações: remover marcação de processamento
/// (usada quando uma tentativa falha e deve ser reprocessada).
/// </summary>
public static class IIdempotencyStoreExtensions
{
    // Interface estática apenas para documentação; implementações concretas podem expor RemoveAsync.
}

