Exemplos práticos de integração e configuração

1) appsettings.json (exemplo)

Incluir a seção de configuração em appsettings.json ou em um arquivo de configuração dedicado:

"Messaging": {
  "Kafka": {
	"BootstrapServers": "localhost:9092",
	"ClientId": "cleanarchmvc",
	"KafkaMaxRetries": 3,
	"RetryBackoff": "00:00:02"
  },
  "RabbitMQ": {
	"HostName": "localhost",
	"Port": 5672,
	"UserName": "guest",
	"Password": "guest"
  }
}

2) Registro no Program.cs (exemplo minimal API / Generic Host)

using CleanArchMvc.Messaging.Kafka;
using CleanArchMvc.Messaging.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// registrar infra genérica (in-memory) para desenvolvimento
builder.Services.AddInMemoryMessaging();

// registrar adapters quando implementados (exemplos):
// builder.Services.AddKafkaMessaging(builder.Configuration);
// builder.Services.AddRabbitMqMessaging(builder.Configuration);

var app = builder.Build();

// ... resto da configuração

3) Exemplo de publicação a partir de um controller/page

public class ExamplePageModel : PageModel
{
	private readonly IMessageProducer _producer;

	public ExamplePageModel(IMessageProducer producer)
	{
		_producer = producer;
	}

	public async Task OnPostAsync()
	{
		var evt = new MyEvent { /* ... */ };
		var envelope = new MessageEnvelope<MyEvent> { Payload = evt, Key = "aggregate-1" };
		await _producer.PublishAsync(envelope);
	}
}

4) Registro de handlers

Na inicialização do aplicativo (Startup/Program) obtenha IMessageConsumer e registre handlers:

var consumer = app.Services.GetRequiredService<IMessageConsumer>();
consumer.RegisterHandler<MyEvent>(new MyEventHandler());
// consumer.StartAsync() é gerenciado pelo HostedService quando AddInMemoryMessaging foi usado

Observações
- Os adapters Kafka/RabbitMQ neste repositório são skeletons. Implementar a lógica concreta
  (mapeamento para messages, commits/acks, particionamento, DLX) é a próxima etapa.
- Use as opções em messaging.example.json para configurar conexão/behavior quando implementar adapters.
