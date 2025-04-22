// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(string name, string description);
    Task<TaskDto> UpdateTaskStatusAsync(int taskId, TEStatus newStatus);
    Task<PaginatedResponse<TaskDto>> GetTasksAsync(PaginationRequest? request = null);
    Task<TaskDto> GetTaskByIdAsync(int taskId);
} 