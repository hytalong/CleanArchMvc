using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Envelope fortemente tipado para mensagens. Contém metadados e payload typed.
/// </summary>
public sealed class MessageEnvelope<T> : IMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public string? EventId { get; init; }
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    public Guid? CausationId { get; init; }

    /// <summary>
    /// Por padrão o MessageType é o nome CLR do payload, mas recomenda-se usar nomes lógicos.
    /// </summary>
    public string MessageType { get; init; } = typeof(T).FullName ?? "Unknown";
    public string Version { get; init; } = "1";
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public string? Key { get; init; }

    /// <summary>
    /// Cabeçalhos auxiliares (ex.: RetryCount, causation info, tracing).
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Payload fortemente tipado.
    /// </summary>
    public T Payload { get; init; } = default!;

    byte[] IMessage.Payload => JsonSerializer.SerializeToUtf8Bytes(Payload, Payload?.GetType() ?? typeof(object));
}

/* Nota:
 - MessageEnvelope<T> fornece uma conversão simples para bytes via System.Text.Json.
 - Em produção, considere injetar/usar um ISerializer para suportar formatos diferentes e
   controlar sensíveis (redaction) em logs.
*/
