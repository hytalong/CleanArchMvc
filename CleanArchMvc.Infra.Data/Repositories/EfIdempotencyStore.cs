using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CleanArchMvc.Messaging.Abstractions;
using CleanArchMvc.Infra.Data.Context;
using CleanArchMvc.Infra.Data.Entities;

namespace CleanArchMvc.Infra.Data.Repositories;

public sealed class EfIdempotencyStore : IIdempotencyStore
{
    private readonly ApplicationDbContext _db;

    public EfIdempotencyStore(ApplicationDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<bool> TryMarkProcessedAsync(Guid messageId, string handlerId, DateTimeOffset processedAt, CancellationToken cancellationToken = default)
    {
        var entry = new ProcessedMessage
        {
            MessageId = messageId,
            HandlerId = handlerId,
            ProcessedAt = processedAt
        };

        try
        {
            _db.ProcessedMessages.Add(entry);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateException)
        {
            // Assume duplicate key -> message already processed
            return false;
        }
    }

    public async Task CleanupOlderThanAsync(DateTimeOffset cutoff, CancellationToken cancellationToken = default)
    {
        // Delete entries older than cutoff
        await _db.Database.ExecuteSqlRawAsync("DELETE FROM ProcessedMessages WHERE ProcessedAt < {0}", new object[] { cutoff }, cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.ProcessedMessages.FindAsync(new object[] { messageId }, cancellationToken).ConfigureAwait(false);
        if (entity != null)
        {
            _db.ProcessedMessages.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
