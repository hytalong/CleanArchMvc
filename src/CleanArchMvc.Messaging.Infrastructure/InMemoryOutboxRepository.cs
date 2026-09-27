using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Infrastructure;

public sealed class InMemoryOutboxRepository : IOutboxRepository
{
    // store with sent flag; GetPending returns items with sent == false
    private readonly ConcurrentDictionary<Guid, (OutboxMessage message, bool sent)> _store = new();

    public Task SaveOutgoingAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _store[message.Id] = (message, false);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        var items = _store.Values.Where(x => !x.sent).Select(x => x.message).Take(batchSize).ToArray();
        return Task.FromResult<IEnumerable<OutboxMessage>>(items);
    }

    public Task MarkSentAsync(Guid outboxMessageId, BrokerMetadata brokerMetadata, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(outboxMessageId, out var tuple))
        {
            _store[outboxMessageId] = (tuple.message, true);
        }
        return Task.CompletedTask;
    }
}
