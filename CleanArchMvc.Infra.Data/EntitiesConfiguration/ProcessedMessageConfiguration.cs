using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CleanArchMvc.Infra.Data.Entities;

namespace CleanArchMvc.Infra.Data.EntitiesConfiguration;

public sealed class ProcessedMessageConfiguration : IEntityTypeConfiguration<ProcessedMessage>
{
    public void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        builder.ToTable("ProcessedMessages");
        builder.HasKey(p => p.MessageId);
        builder.Property(p => p.HandlerId).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ProcessedAt).IsRequired();

        // Unique MessageId ensures atomicity when inserting
        builder.HasIndex(p => p.MessageId).IsUnique();
    }
}
