namespace TaskManagement.Application.DTOs;

public class CreateTaskRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 