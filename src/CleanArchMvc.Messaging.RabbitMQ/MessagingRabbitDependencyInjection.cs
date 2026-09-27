using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.RabbitMQ;

public static class MessagingRabbitDependencyInjection
{
    /// <summary>
    /// Registra opções e adapters RabbitMQ (skeleton). Não ativa o RabbitMQ de verdade —
    /// serve como exemplo de como ligar o adapter quando implementado.
    /// </summary>
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Messaging:RabbitMQ");
        var options = section.Get<RabbitOptions>() ?? new RabbitOptions();

        services.AddSingleton(options);
        services.AddSingleton<RabbitProducerAdapter>();
        services.AddSingleton<IMessageProducer>(sp => sp.GetRequiredService<RabbitProducerAdapter>());
        services.AddSingleton<RabbitConsumerAdapter>();
        services.AddSingleton<IMessageConsumer>(sp => sp.GetRequiredService<RabbitConsumerAdapter>());

        return services;
    }
}
