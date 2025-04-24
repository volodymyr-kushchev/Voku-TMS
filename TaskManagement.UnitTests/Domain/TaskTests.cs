// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using FluentAssertions;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.UnitTests.Domain;

public class TaskTests
{
    [Fact]
    public void CreateTask_WithValidData_ShouldCreateTask()
    {
        // Arrange
        var name = "Test Task";
        var description = "Test Description";

        // Act
        var task = new TaskEntity(name, description);

        // Assert
        task.Name.Should().Be(name);
        task.Description.Should().Be(description);
        task.Status.Should().Be(TEStatus.NotStarted);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        task.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateTask_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var name = string.Empty;
        var description = "Test Description";

        // Act
        var act = () => new TaskEntity(name, description);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Task name cannot be empty");
    }

    [Fact]
    public void CreateTask_WithNameTooLong_ShouldThrowException()
    {
        // Arrange
        var name = new string('a', 201);
        var description = "Test Description";

        // Act
        var act = () => new TaskEntity(name, description);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Task name cannot exceed 200 characters");
    }

    [Fact]
    public void CreateTask_WithDescriptionTooLong_ShouldThrowException()
    {
        // Arrange
        var name = "Test Task";
        var description = new string('a', 1001);

        // Act
        var act = () => new TaskEntity(name, description);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Task description cannot exceed 1000 characters");
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateTask()
    {
        // Arrange
        var task = new TaskEntity("Original Name", "Original Description");
        var newName = "Updated Name";
        var newDescription = "Updated Description";

        // Act
        task.UpdateDetails(newName, newDescription);

        // Assert
        task.Name.Should().Be(newName);
        task.Description.Should().Be(newDescription);
        task.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ChangeStatus_WithValidTransition_ShouldUpdateStatus()
    {
        // Arrange
        var task = new TaskEntity("Test Task", "Test Description");

        // Act
        task.ChangeStatus(TEStatus.InProgress);

        // Assert
        task.Status.Should().Be(TEStatus.InProgress);
        task.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ChangeStatus_WithInvalidTransition_ShouldThrowException()
    {
        // Arrange
        var task = new TaskEntity("Test Task", "Test Description");

        // Act
        var act = () => task.ChangeStatus(TEStatus.Completed);

        // Assert
        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void CanTransitionTo_WithValidStatus_ShouldReturnTrue()
    {
        // Arrange
        var task = new TaskEntity("Test Task", "Test Description");

        // Act
        var canTransition = task.CanTransitionTo(TEStatus.InProgress);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_WithInvalidStatus_ShouldReturnFalse()
    {
        // Arrange
        var task = new TaskEntity("Test Task", "Test Description");

        // Act
        var canTransition = task.CanTransitionTo(TEStatus.Completed);

        // Assert
        canTransition.Should().BeFalse();
    }

    [Fact]
    public void StatusProperties_ShouldReturnCorrectValues()
    {
        // Arrange
        var task = new TaskEntity("Test Task", "Test Description");

        // Assert
        task.IsNotStarted.Should().BeTrue();
        task.IsInProgress.Should().BeFalse();
        task.IsCompleted.Should().BeFalse();

        // Act
        task.ChangeStatus(TEStatus.InProgress);

        // Assert
        task.IsNotStarted.Should().BeFalse();
        task.IsInProgress.Should().BeTrue();
        task.IsCompleted.Should().BeFalse();

        // Act
        task.ChangeStatus(TEStatus.Completed);

        // Assert
        task.IsNotStarted.Should().BeFalse();
        task.IsInProgress.Should().BeFalse();
        task.IsCompleted.Should().BeTrue();
    }
} 