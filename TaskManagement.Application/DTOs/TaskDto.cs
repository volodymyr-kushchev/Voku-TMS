// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TEStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
} 