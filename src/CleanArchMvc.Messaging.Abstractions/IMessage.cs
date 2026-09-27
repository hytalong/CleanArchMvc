using System;
using System.Collections.Generic;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Contrato base para uma mensagem de mensageria.
/// Use MessageEnvelope{T} para mensagens fortemente tipadas na aplicação.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Identificador único da mensagem (geralmente GUID).
    /// Usado para idempotência no consumer.
    /// </summary>
    Guid MessageId { get; }

    /// <summary>
    /// Identificador de evento lógico (opcional).
    /// </summary>
    string? EventId { get; }

    /// <summary>
    /// CorrelationId para traçar chamadas distribuídas.
    /// Deve ser propagado entre produtores/consumidores.
    /// </summary>
    Guid CorrelationId { get; }

    /// <summary>
    /// Identificador da causa (opcional) — por exemplo, messageId anterior.
    /// </summary>
    Guid? CausationId { get; }

    /// <summary>
    /// Tipo lógico da mensagem (ex.: "ProductCreated").
    /// </summary>
    string MessageType { get; }

    /// <summary>
    /// Versão do contrato. Importante para evolução dos eventos.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Timestamp em UTC de criação da mensagem.
    /// </summary>
    DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Chave de particionamento / routing key — usada para ordering quando aplicável.
    /// </summary>
    string? Key { get; }

    /// <summary>
    /// Cabeçalhos e metadados adicionais.
    /// </summary>
    IReadOnlyDictionary<string, string> Headers { get; }

    /// <summary>
    /// Payload serializado (bytes). Implementações concretas e helpers de serialização
    /// devem converter entre tipos fortes e esta propriedade.
    /// </summary>
    byte[] Payload { get; }
}

/*
Exemplo de uso (producer):

var envelope = new MessageEnvelope<MyEvent>
{
    Payload = new MyEvent { ... },
    CorrelationId = correlationId,
    Key = aggregateId.ToString()
};

await producer.PublishAsync(envelope);

*/
