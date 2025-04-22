// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore.InMemory;

namespace TaskManagement.UnitTests.Infrastructure.Services;

public class TaskServiceTests : IAsyncLifetime
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly ApplicationDbContext _context;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _taskService = new TaskService(_context, _publishEndpointMock.Object);
    }

    public Task DisposeAsync() => _context.Database.EnsureDeletedAsync();
    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateTaskAsync_WithValidData_ShouldCreateTask()
    {
        var name = "Test Task";
        var description = "Test Description";

        var result = await _taskService.CreateTaskAsync(name, description);

        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Description.Should().Be(description);
        result.Status.Should().Be(TEStatus.NotStarted);

        var taskInDb = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
        taskInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WithValidTransition_ShouldUpdateStatus()
    {
        var task = new TaskEntity { Name = "Test Task", Description = "Test Description", Status = TEStatus.NotStarted };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _taskService.UpdateTaskStatusAsync(task.Id, TEStatus.InProgress);

        result.Should().NotBeNull();
        result.Status.Should().Be(TEStatus.InProgress);

        var updatedTask = await _context.Tasks.FindAsync(task.Id);
        updatedTask?.Status.Should().Be(TEStatus.InProgress);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WithInvalidTransition_ShouldThrowException()
    {
        var task = new TaskEntity { Name = "Test Task", Description = "Test Description", Status = TEStatus.NotStarted };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var act = async () => await _taskService.UpdateTaskStatusAsync(task.Id, TEStatus.Completed);

        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WithNonExistentTask_ShouldThrowException()
    {
        var taskId = 999;
        var act = async () => await _taskService.UpdateTaskStatusAsync(taskId, TEStatus.InProgress);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnAllTasks()
    {
        var tasks = new List<TaskEntity>
        {
            new() { Name = "Task 1", Description = "Description 1", Status = TEStatus.NotStarted },
            new() { Name = "Task 2", Description = "Description 2", Status = TEStatus.InProgress }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        var result = await _taskService.GetAllTasksAsync();

        result.Should().HaveCount(2);
        result.Select(t => t.Name).Should().BeEquivalentTo("Task 1", "Task 2");
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithExistingTask_ShouldReturnTask()
    {
        var task = new TaskEntity { Name = "Test Task", Description = "Test Description", Status = TEStatus.NotStarted };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _taskService.GetTaskByIdAsync(task.Id);

        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Name.Should().Be(task.Name);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithNonExistentTask_ShouldThrowException()
    {
        var taskId = 999;
        var act = async () => await _taskService.GetTaskByIdAsync(taskId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetTasksAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        var tasks = new List<TaskEntity>
        {
            new() { Name = "Task 1", Description = "Description 1", Status = TEStatus.NotStarted },
            new() { Name = "Task 2", Description = "Description 2", Status = TEStatus.InProgress },
            new() { Name = "Task 3", Description = "Description 3", Status = TEStatus.Completed }
        };
        _context.Tasks.AddRange(tasks);
        await _context.SaveChangesAsync();

        var request = new PaginationRequest { PageNumber = 1, PageSize = 2 };

        var result = await _taskService.GetTasksAsync(request);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }
}
