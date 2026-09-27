using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CleanArchMvc.Messaging.Abstractions;
using Polly;
using Polly.Retry;

namespace CleanArchMvc.Messaging.Infrastructure;

public sealed class InMemoryProducer : IMessageProducer
{
    private readonly InMemoryTransport _transport;
    private readonly RetryPolicy _policyModel;
    private readonly AsyncRetryPolicy _pollyPolicy;

    public InMemoryProducer(InMemoryTransport transport, RetryPolicy? policyModel = null)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _policyModel = policyModel ?? new RetryPolicy();

        _pollyPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: Math.Max(1, _policyModel.MaxAttempts - 1),
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(Math.Min((long)(_policyModel.InitialDelay.TotalMilliseconds * Math.Pow(_policyModel.BackoffFactor, attempt - 1)), (long)_policyModel.MaxDelay.TotalMilliseconds))
            );
    }

    public async Task<PublishResult> PublishAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _pollyPolicy.ExecuteAsync(async ct =>
            {
                await _transport.PublishAsync(message).ConfigureAwait(false);
            }, cancellationToken).ConfigureAwait(false);

            return new PublishResult { Success = true };
        }
        catch (Exception ex)
        {
            return new PublishResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<IEnumerable<PublishResult>> PublishManyAsync(IEnumerable<IMessage> messages, PublishOptions? options = null, CancellationToken cancellationToken = default)
    {
        var results = new List<PublishResult>();
        foreach (var m in messages)
        {
            results.Add(await PublishAsync(m, cancellationToken).ConfigureAwait(false));
        }
        return results;
    }
}
