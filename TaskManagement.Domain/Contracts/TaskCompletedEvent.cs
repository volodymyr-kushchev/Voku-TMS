namespace TaskManagement.Domain.Contracts;

public record TaskCompletedEvent
{
    public int TaskId { get; init; }
    public string TaskName { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
} 