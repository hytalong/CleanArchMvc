Visão geral

Esta pasta contém a especificação e uma implantação de referência em memória
para uma camada de mensageria agnóstica ao broker (Kafka / RabbitMQ).
O objetivo é permitir que a aplicação publique/consuma mensagens sem
ficar acoplada ao broker; adapters concretos serão adicionados posteriormente.

Componentes principais

- CleanArchMvc.Messaging.Abstractions: contratos públicos (IMessage, MessageEnvelope<T>,
  IMessageProducer, IMessageConsumer, IMessageHandler<T>, RetryPolicy, IIdempotencyStore,
  IOutboxRepository, IDeadLetterStore).
- CleanArchMvc.Messaging.Infrastructure: implementação em memória para desenvolvimento e testes
  (InMemoryTransport, InMemoryProducer, HostedConsumerService, OutboxDispatcher,
  stores em memória para idempotência/DLQ/Outbox).
- tests: projeto de testes usando xUnit cobrindo publish/consume/retry/dlq/outbox/idempotência.

Quickstart (uso na aplicação)

1) Registrar serviços (já adicionado em IoC):

   services.AddInMemoryMessaging();

   - Em produção, substitua as implementações em memória por adapters (Kafka/RabbitMQ)
	 implementando as mesmas interfaces.

2) Publicar mensagem (exemplo):

   var envelope = new MessageEnvelope<MyEvent>
   {
	   Payload = myEvent,
	   CorrelationId = correlationId,
	   Key = aggregateId.ToString()
   };

   await messageProducer.PublishAsync(envelope);

3) Registrar handler (exemplo):

   consumer.RegisterHandler<MyEvent>(new MyEventHandler(), new ConsumerOptions { ConsumerGroup = "service-a" });

Principais decisões de design

- Envelope padrão contém MessageId, CorrelationId, CausationId, MessageType, Version, Key,
  Headers e Payload. MessageId é chave de idempotência primária.
- A abstração expõe Key e ConsumerGroup; conceitos específicos (partition, offset,
  exchange, queue) são mantidos em BrokerMetadata retornado pelo PublishResult.
- Idempotência: IIdempotencyStore com implementação EFCore (EfIdempotencyStore) para uso
  em produção. TryMarkProcessedAsync deve ser atômico (INSERT único com unique index).
- Retry: políticas configuráveis por handler (RetryPolicy). Producer e consumer podem
  aplicar retry/backoff; exemplo usa Polly no producer.
- DLQ: IDeadLetterStore que persiste mensagem original + motivo. Reprocessamento via API Requeue.
- Outbox: IOutboxRepository + OutboxDispatcher para padrão Outbox (recomendado quando
  operações de banco e publicação precisam ser atômicas).

Diferenças Kafka vs RabbitMQ (como tratar)

- Kafka fornece partitions, offsets e consumer groups com ordering por partition. Mapear
  envelope.Key → partition. Exactly-once exige features do Kafka (idempotent producer/transactions).
- RabbitMQ usa exchanges/queues/routing keys e não garante ordering com competing consumers.
  Para ordering forte em RabbitMQ, usar single-queue + single-consumer por chave ou sharding.
- Não tentar normalizar partitions e exchanges; fornecer mapeamentos e expor BrokerMetadata
  para operações que precisem de informações específicas do broker.

Observability e operação

- Propagar CorrelationId em todos os logs/traces.
- Em cada passo emitir eventos de log estruturado: MessagePublished, MessageReceived,
  HandlerStart, HandlerSuccess, HandlerFailure, MessageSentToDLQ.
- Expor métricas: messages_published_total, messages_processed_total, messages_dlq_total,
  message_processing_duration_seconds, publish_retries_total.

Testes

- Projeto tests/CleanArchMvc.Messaging.Tests contém cenários para publish, consume,
  handler success/failure, idempotência, DLQ e Outbox dispatcher (em memória).

Próximos passos sugeridos

1. Revisar e aprovar o design das abstrações.
2. Implementar adapters concretos: primeiro Kafka (conforme prioridade), depois RabbitMQ.
3. Integrar OpenTelemetry e exporter de métricas (Prometheus).
4. Preparar migrations adicionais (Outbox, DeadLetters) e ajustar políticas de retenção.

Observações finais

Este código inicial visa prover um ciclo de desenvolvimento seguro (in-memory) e
os contratos necessários para implementar adapters concretos sem acoplar a camada de aplicação
ao broker escolhido. Ajustes poderão ser feitos conforme requisitos operacionais.
