using System;
using System.Collections.Generic;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Infrastructure;

internal sealed class TransportMessage : IMessage
{
    public TransportMessage(IMessage source, IReadOnlyDictionary<string, string>? overrideHeaders = null)
    {
        MessageId = source.MessageId;
        EventId = source.EventId;
        CorrelationId = source.CorrelationId;
        CausationId = source.CausationId;
        MessageType = source.MessageType;
        Version = source.Version;
        Timestamp = source.Timestamp;
        Key = source.Key;
        Payload = source.Payload;
        Headers = overrideHeaders ?? source.Headers;
    }

    public Guid MessageId { get; }
    public string? EventId { get; }
    public Guid CorrelationId { get; }
    public Guid? CausationId { get; }
    public string MessageType { get; }
    public string Version { get; }
    public DateTimeOffset Timestamp { get; }
    public string? Key { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public byte[] Payload { get; }
}
