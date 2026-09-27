using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Kafka;

public static class MessagingKafkaDependencyInjection
{
    /// <summary>
    /// Registra opções e adapters Kafka (skeleton). Não ativa o Kafka de verdade —
    /// serve como exemplo de como ligar o adapter quando implementado.
    /// </summary>
    public static IServiceCollection AddKafkaMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Messaging:Kafka");
        var options = section.Get<KafkaOptions>() ?? new KafkaOptions();

        // Registrar opções e adapters skeleton. Implementações concretas devem usar Confluent.Kafka
        services.AddSingleton(options);
        services.AddSingleton<KafkaProducerAdapter>();
        services.AddSingleton<IMessageProducer>(sp => sp.GetRequiredService<KafkaProducerAdapter>());
        services.AddSingleton<KafkaConsumerAdapter>();
        services.AddSingleton<IMessageConsumer>(sp => sp.GetRequiredService<KafkaConsumerAdapter>());

        return services;
    }
}
