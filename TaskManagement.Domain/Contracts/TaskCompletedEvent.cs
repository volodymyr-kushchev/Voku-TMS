// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

namespace TaskManagement.Domain.Contracts;

public record TaskCompletedEvent
{
    public int TaskId { get; init; }
    public string TaskName { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
} 