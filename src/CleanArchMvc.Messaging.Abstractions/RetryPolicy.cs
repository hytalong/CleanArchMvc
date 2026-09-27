using System;

namespace CleanArchMvc.Messaging.Abstractions;

/// <summary>
/// Modelo simples de política de retry. Para lógica avançada use bibliotecas como Polly
/// na implementação do producer/consumer.
/// </summary>
public sealed class RetryPolicy
{
    /// <summary>
    /// Número máximo de tentativas totais (inclui primeira tentativa).
    /// </summary>
    public int MaxAttempts { get; init; } = 5;

    /// <summary>
    /// Delay inicial entre tentativas.
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Fator de multiplicação aplicado ao delay (exponential).
    /// </summary>
    public double BackoffFactor { get; init; } = 2.0; // exponential

    /// <summary>
    /// Delay máximo permitido entre tentativas.
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromMinutes(1);
}

