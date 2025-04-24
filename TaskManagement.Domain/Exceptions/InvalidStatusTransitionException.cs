// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid task status transition is attempted
/// </summary>
public class InvalidStatusTransitionException : DomainException
{
    public TEStatus CurrentStatus { get; }
    public TEStatus NewStatus { get; }

    public InvalidStatusTransitionException(TEStatus currentStatus, TEStatus newStatus)
        : base($"Cannot transition task from {currentStatus} to {newStatus}")
    {
        CurrentStatus = currentStatus;
        NewStatus = newStatus;
    }
} 