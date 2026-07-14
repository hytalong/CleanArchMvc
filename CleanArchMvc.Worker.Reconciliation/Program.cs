using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) => cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true))
    .ConfigureServices((context, services) =>
    {
        // Reuse IoC registration from the solution (Application/Infra)
        services.AddInfrastructure(context.Configuration);

        var kafkaSection = context.Configuration.GetSection("Kafka");
        var producerConfig = new ProducerConfig { BootstrapServers = kafkaSection["BootstrapServers"] };
        services.AddSingleton(producerConfig);

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = kafkaSection["BootstrapServers"],
            GroupId = kafkaSection["GroupId"] ?? "reconciliation-consumers",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        services.AddSingleton(consumerConfig);

        // Background services
        services.AddHostedService<CleanArchMvc.Worker.Reconciliation.Producers.FileProducerService>();
        services.AddHostedService<CleanArchMvc.Worker.Reconciliation.Consumers.ReconciliationConsumerService>();
    })
    .UseSerilog((ctx, lc) => lc.WriteTo.Console());

var host = builder.Build();

await host.RunAsync();
