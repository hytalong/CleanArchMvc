using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Infrastructure;

public sealed class InMemoryDeadLetterStore : IDeadLetterStore
{
    private readonly ConcurrentDictionary<Guid, DeadLetterMessage> _store = new();

    public Task SaveAsync(DeadLetterMessage message, CancellationToken cancellationToken = default)
    {
        _store[message.Id] = message;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<DeadLetterMessage>> GetAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var items = _store.Values.Skip((page - 1) * pageSize).Take(pageSize).ToArray();
        return Task.FromResult<IEnumerable<DeadLetterMessage>>(items);
    }

    public Task RequeueAsync(Guid deadLetterId, CancellationToken cancellationToken = default)
    {
        // Policy: remove from DLQ; requeue should be implemented by caller using producer.
        _store.TryRemove(deadLetterId, out _);
        return Task.CompletedTask;
    }
}
