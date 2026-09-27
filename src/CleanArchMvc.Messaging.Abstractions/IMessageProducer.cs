using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Abstração para produtores. Não deve expor detalhes do broker.
/// Implementações concretas traduzirão IMessage para primitives do broker.
/// </summary>
public interface IMessageProducer
{
    /// <summary>
    /// Publica uma mensagem assincronamente.
    /// Retorna PublishResult com metadata broker-specific quando disponível.
    /// </summary>
    Task<PublishResult> PublishAsync(IMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publica muitas mensagens em lote.
    /// </summary>
    Task<IEnumerable<PublishResult>> PublishManyAsync(IEnumerable<IMessage> messages, PublishOptions? options = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Opções de publicação, por exemplo para pedidos de ordenação.
/// </summary>
public sealed class PublishOptions
{
    /// <summary>
    /// Indica que a ordem deve ser preservada quando possível (p.ex. usar a mesma partition).
    /// Implementações podem ignorar se não suportarem.
    /// </summary>
    public bool EnsureOrdered { get; set; } = false;
}

