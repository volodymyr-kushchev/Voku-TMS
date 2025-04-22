// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public TEStatus CurrentStatus { get; }
    public TEStatus NewStatus { get; }

    public InvalidStatusTransitionException(TEStatus currentStatus, TEStatus newStatus)
        : base($"Invalid status transition from {currentStatus} to {newStatus}")
    {
        CurrentStatus = currentStatus;
        NewStatus = newStatus;
    }
} 