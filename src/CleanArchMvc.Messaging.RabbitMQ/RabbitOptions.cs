using System;

namespace CleanArchMvc.Messaging.RabbitMQ;

public sealed class RabbitOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    // Mais opções específicas (exchange type, DLX, TTL) podem ser adicionadas.
}
