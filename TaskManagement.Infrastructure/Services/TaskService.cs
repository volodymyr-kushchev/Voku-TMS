// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using MassTransit;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Contracts;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public TaskService(ApplicationDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<TaskDto> CreateTaskAsync(string name, string description)
    {
        var task = new TaskEntity
        {
            Name = name,
            Description = description,
            Status = TEStatus.NotStarted
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<TaskDto> UpdateTaskStatusAsync(int taskId, TEStatus newStatus)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
            throw new NotFoundException($"Task with ID {taskId} not found.");

        if (!IsValidStatusTransition(task.Status, newStatus))
            throw new InvalidStatusTransitionException(task.Status, newStatus);

        task.Status = newStatus;

        if (newStatus == TEStatus.Completed)
        {
            await _publishEndpoint.Publish(new TaskCompletedEvent
            {
                TaskId = task.Id,
                TaskName = task.Name,
                CompletedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        return MapToDto(task);
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        var tasks = await _context.Tasks.ToListAsync();
        return tasks.Select(MapToDto);
    }

    public async Task<TaskDto> GetTaskByIdAsync(int taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
            throw new NotFoundException($"Task with ID {taskId} not found.");

        return MapToDto(task);
    }

    public async Task<PaginatedResponse<TaskDto>> GetTasksAsync(PaginationRequest? request = null)
    {
        var query = _context.Tasks.AsQueryable();
        var totalCount = await query.CountAsync();

        if (request?.PageNumber.HasValue == true && request?.PageSize.HasValue == true)
        {
            var items = await query
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value)
                .Select(task => MapToDto(task))
                .ToListAsync();

            return new PaginatedResponse<TaskDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = request.PageNumber.Value,
                PageSize = request.PageSize.Value
            };
        }

        var allItems = await query
            .Select(task => MapToDto(task))
            .ToListAsync();

        return new PaginatedResponse<TaskDto>
        {
            Items = allItems,
            TotalCount = totalCount,
            PageNumber = 1,
            PageSize = totalCount
        };
    }

    private static bool IsValidStatusTransition(TEStatus currentStatus, TEStatus newStatus)
    {
        return newStatus switch
        {
            TEStatus.NotStarted => false,
            TEStatus.InProgress => currentStatus == TEStatus.NotStarted,
            TEStatus.Completed => currentStatus == TEStatus.InProgress,
            _ => false
        };
    }

    private static TaskDto MapToDto(TaskEntity task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Status = task.Status
        };
    }
} 