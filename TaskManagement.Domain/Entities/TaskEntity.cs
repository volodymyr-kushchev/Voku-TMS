using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

public class TaskEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TEStatus Status { get; set; }
}