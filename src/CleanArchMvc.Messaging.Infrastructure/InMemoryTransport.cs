using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Infrastructure;

/// <summary>
/// Transporte em memória simples usado para desenvolvimento e testes.
/// Não deve ser usado em produção.
/// </summary>
public sealed class InMemoryTransport
{
    private readonly ConcurrentQueue<IMessage> _queue = new();

    public event Func<IMessage, Task>? OnMessage;

    public Task PublishAsync(IMessage message)
    {
        _queue.Enqueue(message);
        _ = DispatchAsync(message);
        return Task.CompletedTask;
    }

    private async Task DispatchAsync(IMessage message)
    {
        var handler = OnMessage;
        if (handler != null)
        {
            try
            {
                await handler.Invoke(message).ConfigureAwait(false);
            }
            catch
            {
                // transport-level swallow: consumer is responsible for retries/DLQ
            }
        }
    }
}
