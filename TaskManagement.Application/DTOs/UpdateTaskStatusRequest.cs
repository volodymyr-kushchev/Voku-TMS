using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.DTOs;

public class UpdateTaskStatusRequest
{
    public TEStatus Status { get; set; }
} 