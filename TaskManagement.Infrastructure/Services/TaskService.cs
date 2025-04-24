// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using MassTransit;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Contracts;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public TaskService(ITaskRepository taskRepository, IPublishEndpoint publishEndpoint)
    {
        _taskRepository = taskRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<TaskDto> CreateTaskAsync(string name, string description)
    {
        var task = new TaskEntity(name, description);
        var createdTask = await _taskRepository.AddAsync(task);
        return MapToDto(createdTask);
    }

    public async Task<TaskDto> UpdateTaskStatusAsync(int taskId, TEStatus newStatus)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new NotFoundException($"Task with ID {taskId} not found.");

        task.ChangeStatus(newStatus);

        if (task.IsCompleted)
        {
            await _publishEndpoint.Publish(new TaskCompletedEvent
            {
                TaskId = task.Id,
                TaskName = task.Name,
                CompletedAt = DateTime.UtcNow
            });
        }

        await _taskRepository.UpdateAsync(task);
        return MapToDto(task);
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return tasks.Select(MapToDto);
    }

    public async Task<TaskDto> GetTaskByIdAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new NotFoundException($"Task with ID {taskId} not found.");

        return MapToDto(task);
    }

    public async Task<PaginatedResponse<TaskDto>> GetTasksAsync(PaginationRequest? request = null)
    {
        if (request?.PageNumber.HasValue == true && request?.PageSize.HasValue == true)
        {
            var result = await _taskRepository.GetPaginatedAsync(
                request.PageNumber.Value,
                request.PageSize.Value);

            return new PaginatedResponse<TaskDto>
            {
                Items = [.. result.Items.Select(MapToDto)],
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        var allTasks = await _taskRepository.GetAllAsync();
        var totalCount = await _taskRepository.GetTotalCountAsync();

        return new PaginatedResponse<TaskDto>
        {
            Items = [.. allTasks.Select(MapToDto)],
            TotalCount = totalCount,
            PageNumber = 1,
            PageSize = totalCount
        };
    }

    private static TaskDto MapToDto(TaskEntity task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Status = task.Status,
            CreatedAt = task.CreatedAt,
            LastModifiedAt = task.LastModifiedAt
        };
    }
} 