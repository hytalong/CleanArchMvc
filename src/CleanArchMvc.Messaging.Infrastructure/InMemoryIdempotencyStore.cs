using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Infrastructure;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _store = new();

    public Task<bool> TryMarkProcessedAsync(Guid messageId, string handlerId, DateTimeOffset processedAt, CancellationToken cancellationToken = default)
    {
        var added = _store.TryAdd(messageId, processedAt);
        return Task.FromResult(added);
    }

    public Task CleanupOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
    {
        foreach (var kv in _store)
        {
            if (kv.Value < cutoff)
            {
                _store.TryRemove(kv.Key, out _);
            }
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(messageId, out _);
        return Task.CompletedTask;
    }
}
