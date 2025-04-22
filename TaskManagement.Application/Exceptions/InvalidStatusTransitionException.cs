// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Exceptions;

public class InvalidStatusTransitionException(TEStatus currentStatus, TEStatus newStatus) : 
    Exception($"Invalid status transition from {currentStatus} to {newStatus}")
{
    public TEStatus CurrentStatus { get; } = currentStatus;
    public TEStatus NewStatus { get; } = newStatus;
} 