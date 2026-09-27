using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CleanArchMvc.Messaging.Infrastructure;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Tests;

public class MessagingTests
{
    private sealed class TestPayload { public string Name { get; set; } = string.Empty; }

    private sealed class TestHandler : IMessageHandler<TestPayload>
    {
        private readonly TaskCompletionSource<bool> _tcs;
        public int Calls; 
        public TestHandler(TaskCompletionSource<bool> tcs) => _tcs = tcs;
        public Task HandleAsync(MessageEnvelope<TestPayload> envelope, HandlerContext context, CancellationToken cancellationToken = default)
        {
            Calls++;
            _tcs.TrySetResult(true);
            return Task.CompletedTask;
        }
    }

    private sealed class FailHandler : IMessageHandler<TestPayload>
    {
        public Task HandleAsync(MessageEnvelope<TestPayload> envelope, HandlerContext context, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("handler failure");
        }
    }

    [Fact]
    public async Task InMemoryProducer_PublishesMessage()
    {
        var transport = new InMemoryTransport();
        var producer = new InMemoryProducer(transport);

        var tcs = new TaskCompletionSource<bool>();
        transport.OnMessage += m => { tcs.TrySetResult(true); return Task.CompletedTask; };

        var envelope = new MessageEnvelope<TestPayload> { Payload = new TestPayload { Name = "abc" } };
        var res = await producer.PublishAsync(envelope);

        Assert.True(res.Success);
        await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.True(tcs.Task.IsCompleted);
    }

    [Fact]
    public async Task HostedConsumerService_InvokesHandler_And_Idempotency()
    {
        var transport = new InMemoryTransport();
        var idemp = new InMemoryIdempotencyStore();
        var dlq = new InMemoryDeadLetterStore();
        var service = new HostedConsumerService(transport, idemp, dlq);

        var tcs = new TaskCompletionSource<bool>();
        var handler = new TestHandler(tcs);
        service.RegisterHandler<TestPayload>(handler, new ConsumerOptions { RetryPolicy = new RetryPolicy { MaxAttempts = 3 } });

        // invoke protected ExecuteAsync via reflection to wire up handlers
        var execute = typeof(HostedConsumerService).GetMethod("ExecuteAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var execTask = (Task)execute.Invoke(service, new object[] { CancellationToken.None })!;

        // publish message
        var envelope = new MessageEnvelope<TestPayload> { Payload = new TestPayload { Name = "x" } };
        await transport.PublishAsync(envelope);

        await Task.WhenAny(tcs.Task, Task.Delay(2000));
        Assert.True(tcs.Task.IsCompleted);

        // Second publish should be ignored by idempotency
        var tcs2 = new TaskCompletionSource<bool>();
        transport.OnMessage += m => { tcs2.TrySetResult(true); return Task.CompletedTask; };
        await transport.PublishAsync(envelope);
        // handler should not be called again (idempotency)
        Assert.False(handler.Calls > 1);
    }

    [Fact]
    public async Task HostedConsumerService_Retries_And_SendsToDLQ()
    {
        var transport = new InMemoryTransport();
        var idemp = new InMemoryIdempotencyStore();
        var dlq = new InMemoryDeadLetterStore();
        var service = new HostedConsumerService(transport, idemp, dlq);

        var handler = new FailHandler();
        service.RegisterHandler<TestPayload>(handler, new ConsumerOptions { RetryPolicy = new RetryPolicy { MaxAttempts = 2, InitialDelay = TimeSpan.FromMilliseconds(10), BackoffFactor = 1.0, MaxDelay = TimeSpan.FromMilliseconds(50) } });

        var execute = typeof(HostedConsumerService).GetMethod("ExecuteAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var execTask = (Task)execute.Invoke(service, new object[] { CancellationToken.None })!;

        var envelope = new MessageEnvelope<TestPayload> { Payload = new TestPayload { Name = "fail" } };
        await transport.PublishAsync(envelope);

        // wait enough time for retries and DLQ
        await Task.Delay(1000);

        var items = await dlq.GetAsync();
        Assert.NotEmpty(items);
    }
}
