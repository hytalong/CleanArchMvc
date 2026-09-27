using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Infrastructure;

/// <summary>
/// Dispatcher que periódicamente consulta o Outbox e publica via IMessageProducer.
/// Implementação simples para desenvolvimento/testes.
/// </summary>
public sealed class OutboxDispatcher : BackgroundService
{
    private readonly IOutboxRepository _outbox;
    private readonly IMessageProducer _producer;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(5);

    public OutboxDispatcher(IOutboxRepository outbox, IMessageProducer producer)
    {
        _outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pending = (await _outbox.GetPendingAsync(50, stoppingToken).ConfigureAwait(false)).ToArray();
                foreach (var msg in pending)
                {
                    var res = await _producer.PublishAsync(msg.Message, stoppingToken).ConfigureAwait(false);
                    if (res.Success)
                    {
                        await _outbox.MarkSentAsync(msg.Id, res.BrokerMetadata ?? new BrokerMetadata(), stoppingToken).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                // swallow; next loop will retry
            }

            await Task.Delay(_pollInterval, stoppingToken).ConfigureAwait(false);
        }
    }
}
