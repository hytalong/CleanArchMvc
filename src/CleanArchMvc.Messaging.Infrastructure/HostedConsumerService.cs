using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Infrastructure;

/// <summary>
/// Hosted service que utiliza InMemoryTransport para receber mensagens e despachar para handlers registrados.
/// Serve como implementação genérica para desenvolvimento e testes.
/// </summary>
public sealed class HostedConsumerService : BackgroundService, IMessageConsumer
{
    private readonly InMemoryTransport _transport;
    private readonly IIdempotencyStore? _idempotencyStore;
    private readonly IDeadLetterStore? _deadLetterStore;
    private readonly ConcurrentDictionary<string, (object handler, ConsumerOptions? options)> _handlers = new();

    public HostedConsumerService(InMemoryTransport transport, IIdempotencyStore? idempotencyStore = null, IDeadLetterStore? deadLetterStore = null)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _idempotencyStore = idempotencyStore;
        _deadLetterStore = deadLetterStore;
    }

    public void RegisterHandler<T>(IMessageHandler<T> handler, ConsumerOptions? options = null)
    {
        var key = typeof(T).FullName ?? typeof(T).Name;
        _handlers[key] = (handler!, options);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _transport.OnMessage += async (message) => await ProcessMessageAsync(message, stoppingToken).ConfigureAwait(false);
        return Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        // localizar handler baseado no message type
        foreach (var kv in _handlers)
        {
            var handlerTypeName = kv.Key;
            if (message.MessageType == null)
                continue;

            if (!message.MessageType.Contains(handlerTypeName, StringComparison.Ordinal) && !handlerTypeName.Contains(message.MessageType, StringComparison.Ordinal))
                continue;

            var (handlerObj, options) = kv.Value;

            // infere T de IMessageHandler<T>
            var handlerInterface = handlerObj.GetType().GetInterfaces();
            var generic = System.Array.Find(handlerInterface, i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
            if (generic == null)
                continue;

            var msgType = generic.GetGenericArguments()[0];
            var payloadObj = Serializer.Deserialize(message.Payload, msgType);

            var ctx = new HandlerContext { HandlerId = handlerObj.GetType().FullName, Attempt = 1 };

            // idempotency reservation: tenta marcar para evitar concorrência
            if (_idempotencyStore != null)
            {
                var first = await _idempotencyStore.TryMarkProcessedAsync(message.MessageId, ctx.HandlerId ?? "handler", DateTimeOffset.UtcNow, cancellationToken).ConfigureAwait(false);
                if (!first)
                {
                    // já processado por outra instância
                    return;
                }
            }

            try
            {
                // construir envelope forte para invocar HandleAsync
                var envelopeType = typeof(MessageEnvelope<>).MakeGenericType(msgType);
                var envelope = Activator.CreateInstance(envelopeType)!;

                // set props via reflection
                void SetProp(string name, object? value)
                {
                    var prop = envelopeType.GetProperty(name);
                    if (prop != null && prop.CanWrite)
                        prop.SetValue(envelope, value);
                }

                SetProp("MessageId", message.MessageId);
                SetProp("EventId", message.EventId);
                SetProp("CorrelationId", message.CorrelationId);
                SetProp("CausationId", message.CausationId);
                SetProp("MessageType", message.MessageType);
                SetProp("Version", message.Version);
                SetProp("Timestamp", message.Timestamp);
                SetProp("Key", message.Key);
                SetProp("Headers", message.Headers);
                SetProp("Payload", payloadObj);

                var method = handlerObj.GetType().GetMethod("HandleAsync");
                if (method != null)
                {
                    var task = (Task)method.Invoke(handlerObj, new[] { envelope, ctx, cancellationToken })!;
                    await task.ConfigureAwait(false);
                    // on success: nothing more (idempotency record marks it processed)
                }
            }
            catch (Exception ex)
            {
                // em caso de falha, desfazer marcação para permitir reprocessamento
                if (_idempotencyStore != null)
                {
                    try { await (_idempotencyStore as dynamic).RemoveAsync(message.MessageId).ConfigureAwait(false); } catch { }
                }

                // obter política de retry
                var policy = options?.RetryPolicy ?? new RetryPolicy();
                var currentAttempt = 1;
                if (message.Headers != null && message.Headers.TryGetValue("RetryCount", out var rc))
                {
                    if (int.TryParse(rc, out var val)) currentAttempt = val;
                }

                if (currentAttempt < policy.MaxAttempts)
                {
                    var nextAttempt = currentAttempt + 1;
                    // calcular delay exponencial
                    var delayMs = Math.Min((long)(policy.InitialDelay.TotalMilliseconds * Math.Pow(policy.BackoffFactor, currentAttempt - 1)), (long)policy.MaxDelay.TotalMilliseconds);
                    var newHeaders = new Dictionary<string, string>(message.Headers);
                    newHeaders["RetryCount"] = nextAttempt.ToString();

                    // re-enfileirar após delay
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(delayMs), cancellationToken).ConfigureAwait(false);
                            var tm = new TransportMessage(message, newHeaders);
                            await _transport.PublishAsync(tm).ConfigureAwait(false);
                        }
                        catch
                        {
                            // swallow
                        }
                    });
                }
                else
                {
                    // excedeu tentativas -> salvar DLQ
                    try
                    {
                        var dlq = new CleanArchMvc.Messaging.Abstractions.DeadLetterMessage
                        {
                            Id = Guid.NewGuid(),
                            OriginalMessage = message,
                            Reason = ex.Message,
                            AttemptCount = currentAttempt,
                            FailedAt = DateTimeOffset.UtcNow,
                            Origin = "InMemory"
                        };

                        if (_deadLetterStore != null)
                        {
                            await _deadLetterStore.SaveAsync(dlq, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch
                    {
                        // swallow
                    }
                }
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
