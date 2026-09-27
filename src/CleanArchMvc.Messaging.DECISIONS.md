Decisões, Trade-offs e Itens para Revisão da Equipe

Resumo

Este documento resume as principais decisões arquiteturais relativas à camada de mensageria genérica
implementada inicialmente em memória e os trade-offs que precisam ser validados pela equipe antes de
seguir para a implementação de adapters concretos (Kafka primeiro).

1) Outbox: agora vs depois
- Opções:
  - Implementar Outbox desde o início (na mesma transação que operações DB): garante atomicidade e reduz
	risco de dual-write. Aumenta complexidade e exige migrations/ops adicionais.
  - Postergar Outbox para segunda fase: entrega mais rápida da infraestrutura de mensageria, menor impacto
	imediato ao schema DB, porém deixa risco de inconsistência em cenários DB + evento.
- Recomendação inicial: se a aplicação frequentemente realiza escrita em DB seguida de publicação de evento,
  adotar Outbox agora. Caso contrário, adiar e documentar os pontos onde Outbox será necessário.

2) Janela de retenção (Idempotência / ProcessedMessages)
- Proposta padrão: 30 dias.
- Trade-off:
  - Janela curta (ex.: 7 dias) reduz armazenamento mas pode permitir reprocessamentos idênticos após expiração.
  - Janela longa (ex.: 90 dias) aumenta armazenamento e custo, melhora segurança contra duplicates tardios.
- Pergunta para equipe: qual janela atende requisitos legais/operacionais?

3) Políticas de retry (defaults)
- Producer (publicação):
  - MaxAttempts: 5
  - InitialDelay: 1s
  - BackoffFactor: 2.0
  - MaxDelay: 1min

- Consumer (processamento):
  - Immediate retries (rápidos) até 2 tentativas
  - Delayed retries com backoff até 5 tentativas (configurável por handler)
  - Headers: "RetryCount" para contar tentativas

- Recomendações:
  - Políticas por tipo de mensagem: handlers críticos podem ter políticas mais agressivas;
  - Evitar loops infinitos: mover para DLQ quando exceder MaxAttempts.

4) DLQ: comportamento e reprocessamento
- Critérios para enviar para DLQ: excedeu tentativas, erro de schema, poison message.
- O DLQ preserva mensagem original + headers + motivo + timestamp + origem.
- Reprocessamento manual via API/operador; requeue automatizado deve verificar idempotência antes.

5) Ordering e Particionamento
- Decisão: não mascarar conceitos broker-specific. Abstração expõe `Key` e `ConsumerGroup`.
- Kafka: ordering por partition (garantido se Key sempre mapear para mesma partition).
- RabbitMQ: sem garantia forte de ordering com competing consumers; para ordering forte usar single-consumer queue ou sharding por key.

6) Exactly-once vs At-least-once
- Decisão prática: oferecer at-least-once como padrão e usar idempotência para garantir efeitos únicos.
- Exactly-once requer suporte do broker (Kafka transactions) e Outbox/coordenação; será considerado para casos específicos.

7) Segurança
- Não logar payloads sem política de redaction.
- Armazenar secrets em config/secret manager; não commitá-los no repo.

8) Observability
- Requisitos mínimos:
  - Logs estruturados (MessageId, CorrelationId, EventType, Handler, AttemptCount, Outcome)
  - Métricas: published, processed, failed, dlq, processing_duration
  - Tracing: propagar CorrelationId e integrar com OpenTelemetry futuramente

9) Implementação gradual sugerida
 1. Confirmar Outbox: sim/não. Se sim, criar migration Outbox e integrar dispatcher.
 2. Concordar com janela de retenção (ProcessedMessages).
 3. Concordar com políticas de retry default.
 4. Implementar adapter Kafka (prioridade) e validar ordering/throughput em testes de integração.
 5. Implementar adapter RabbitMQ se necessário.

Perguntas abertas para a equipe
- A aplicação atualmente tem cenários críticos de dual-write que justificam Outbox imediato?
- Qual janela de retenção desejada para ProcessedMessages (dias)?
- Qual política de retry padrão da organização (MaxAttempts e backoff)?
- Devemos expor endpoint administrativo para reprocessar DLQ via UI/API?

Notas finais

Os artefatos já gerados (projeto de abstractions, infra em memória, ef-store para idempotência,
Outbox em memória e projetos-skeleton para Kafka/RabbitMQ) permitem que a equipe valide os
contratos sem acoplamento a um broker específico. Após decisões acima, avançar para a implementação
do adapter Kafka conforme prioridade.
