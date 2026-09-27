using System;

namespace CleanArchMvc.Infra.Data.Entities;

public sealed class ProcessedMessage
{
    public Guid MessageId { get; set; }
    public string HandlerId { get; set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; set; }
}
