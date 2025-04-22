// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

namespace TaskManagement.Application.DTOs;

/// <summary>
/// Request model for creating a new task
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// The name of the task
    /// </summary>
    /// <example>Implement user authentication</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the task
    /// </summary>
    /// <example>Implement JWT-based authentication with refresh tokens</example>
    public string Description { get; set; } = string.Empty;
} 