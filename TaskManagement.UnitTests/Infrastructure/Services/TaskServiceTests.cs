// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using FluentAssertions;
using MassTransit;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Exceptions;
using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Services;
using TaskManagement.UnitTests.Common;

namespace TaskManagement.UnitTests.Infrastructure.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _taskService = new TaskService(_taskRepositoryMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task CreateTaskAsync_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var name = "Test Task";
        var description = "Test Description";
        var task = TestData.CreateTask(name, description);
        _taskRepositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskEntity>()))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.CreateTaskAsync(name, description);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Description.Should().Be(description);
        result.Status.Should().Be(TEStatus.NotStarted);
        _taskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<TaskEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WithValidTransition_ShouldUpdateStatus()
    {
        // Arrange
        var task = TestData.CreateTask();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);
        _taskRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<TaskEntity>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(task.Id, TEStatus.InProgress);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(TEStatus.InProgress);
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WithInvalidTransition_ShouldThrowException()
    {
        // Arrange
        var task = TestData.CreateTask();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        // Act
        var act = async () => await _taskService.UpdateTaskStatusAsync(task.Id, TEStatus.Completed);

        // Assert
        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskEntity>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_WithNonExistentTask_ShouldThrowException()
    {
        // Arrange
        var taskId = 999;
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskEntity?)null);

        // Act
        var act = async () => await _taskService.UpdateTaskStatusAsync(taskId, TEStatus.InProgress);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<TaskEntity>()), Times.Never);
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnAllTasks()
    {
        // Arrange
        var tasks = TestData.CreateTasks(2).ToList();
        _taskRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(tasks);

        // Act
        var result = await _taskService.GetAllTasksAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(t => t.Name).Should().BeEquivalentTo("Task 1", "Task 2");
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithExistingTask_ShouldReturnTask()
    {
        // Arrange
        var task = TestData.CreateTask();
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        // Act
        var result = await _taskService.GetTaskByIdAsync(task.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Name.Should().Be(task.Name);
    }

    [Fact]
    public async Task GetTaskByIdAsync_WithNonExistentTask_ShouldThrowException()
    {
        // Arrange
        var taskId = 999;
        _taskRepositoryMock.Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskEntity?)null);

        // Act
        var act = async () => await _taskService.GetTaskByIdAsync(taskId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetTasksAsync_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        var tasks = TestData.CreateTasks(3).ToList();
        var paginatedResult = new PaginatedResult<TaskEntity>
        {
            Items = tasks.Take(2),
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 2
        };

        _taskRepositoryMock.Setup(x => x.GetPaginatedAsync(1, 2))
            .ReturnsAsync(paginatedResult);

        var request = new PaginationRequest { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await _taskService.GetTasksAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetTasksAsync_WithoutPagination_ShouldReturnAllTasks()
    {
        // Arrange
        var tasks = TestData.CreateTasks(3).ToList();
        _taskRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(tasks);
        _taskRepositoryMock.Setup(x => x.GetTotalCountAsync())
            .ReturnsAsync(3);

        // Act
        var result = await _taskService.GetTasksAsync();

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(3);
    }
}
