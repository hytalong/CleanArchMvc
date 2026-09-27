using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using CleanArchMvc.Messaging.Infrastructure;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Tests;

public class OutboxDispatcherTests
{
    private sealed class TestPayload { public string Name { get; set; } = string.Empty; }

    [Fact]
    public async Task OutboxDispatcher_Publishes_PendingMessages_And_MarksSent()
    {
        var transport = new InMemoryTransport();
        var producer = new InMemoryProducer(transport);
        var outbox = new InMemoryOutboxRepository();
        var dispatcher = new OutboxDispatcher(outbox, producer);

        var tcs = new TaskCompletionSource<bool>();
        transport.OnMessage += msg => { tcs.TrySetResult(true); return Task.CompletedTask; };

        var envelope = new MessageEnvelope<TestPayload> { Payload = new TestPayload { Name = "o" } };
        var outboxMessage = new OutboxMessage { Message = envelope };
        await outbox.SaveOutgoingAsync(outboxMessage);

        var exec = typeof(OutboxDispatcher).GetMethod("ExecuteAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var task = (Task)exec.Invoke(dispatcher, new object[] { CancellationToken.None })!;

        await Task.WhenAny(tcs.Task, Task.Delay(2000));
        Assert.True(tcs.Task.IsCompleted, "Message was not published by dispatcher");

        var pending = await outbox.GetPendingAsync(10);
        Assert.Empty(pending);
    }
}
