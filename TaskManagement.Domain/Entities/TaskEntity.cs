// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using TaskManagement.Domain.Enums;
using TaskManagement.Domain.Exceptions;

namespace TaskManagement.Domain.Entities;

public class TaskEntity
{
    // For EF Core
    protected TaskEntity() { }

    internal TaskEntity(string name, string description, TEStatus status)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Task name cannot be empty");

        if (name.Length > 200)
            throw new DomainException("Task name cannot exceed 200 characters");

        if (description.Length > 1000)
            throw new DomainException("Task description cannot exceed 1000 characters");

        Name = name;
        Description = description;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
    }

    public TaskEntity(string name, string description)
        : this(name, description, TEStatus.NotStarted)
    {
    }

    public int Id { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public TEStatus Status { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime LastModifiedAt { get; protected set; }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Task name cannot be empty");

        if (name.Length > 200)
            throw new DomainException("Task name cannot exceed 200 characters");

        if (description.Length > 1000)
            throw new DomainException("Task description cannot exceed 1000 characters");

        Name = name;
        Description = description;
        LastModifiedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(TEStatus newStatus)
    {
        if (!IsValidStatusTransition(Status, newStatus))
            throw new InvalidStatusTransitionException(Status, newStatus);

        Status = newStatus;
        LastModifiedAt = DateTime.UtcNow;
    }

    public bool CanTransitionTo(TEStatus newStatus)
    {
        return IsValidStatusTransition(Status, newStatus);
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

    public bool IsCompleted => Status == TEStatus.Completed;
    public bool IsInProgress => Status == TEStatus.InProgress;
    public bool IsNotStarted => Status == TEStatus.NotStarted;
}