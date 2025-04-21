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