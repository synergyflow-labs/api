namespace SynergyFlow.Domain.Common;

public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Type { get; set; }

    public required string Content { get; set; }

    public DateTime OccurredOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }
}
