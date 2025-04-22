// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using System.Text.Json.Serialization;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

/// <summary>
/// Request model for patching a task's status
/// </summary>
public class PatchTaskStatusRequest
{
    /// <summary>
    /// The new status for the task
    /// </summary>
    /// <example>InProgress</example>
    [JsonPropertyName("status")]
    public TEStatus Status { get; set; }
} 