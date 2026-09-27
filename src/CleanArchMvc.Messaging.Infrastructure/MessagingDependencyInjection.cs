using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CleanArchMvc.Messaging.Abstractions;

namespace CleanArchMvc.Messaging.Infrastructure;

public static class MessagingDependencyInjection
{
    /// <summary>
    /// Registra infraestrutura de mensageria em memória (development/test).
    /// Não registra adapters concretos de broker.
    /// </summary>
    public static IServiceCollection AddInMemoryMessaging(this IServiceCollection services)
    {
        services.TryAddSingleton<InMemoryTransport>();
        services.TryAddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        services.TryAddSingleton<IDeadLetterStore, InMemoryDeadLetterStore>();
        services.TryAddSingleton<IOutboxRepository, InMemoryOutboxRepository>();
        services.TryAddSingleton<IMessageProducer, InMemoryProducer>();

        // Hosted consumer (background) that also expõe IMessageConsumer
        services.TryAddSingleton<HostedConsumerService>();
        services.AddSingleton<IMessageConsumer>(sp => sp.GetRequiredService<HostedConsumerService>());
        services.AddHostedService(sp => sp.GetRequiredService<HostedConsumerService>());

        // Outbox dispatcher
        services.AddHostedService<OutboxDispatcher>();

        return services;
    }
}
