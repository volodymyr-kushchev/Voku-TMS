// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using TaskManagement.Domain.Entities;

namespace TaskManagement.UnitTests.Common;

public static class TestData
{
    public static TaskEntity CreateTask(string name = "Test Task", string description = "Test Description")
    {
        return new TaskEntity(name, description);
    }

    public static IEnumerable<TaskEntity> CreateTasks(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new TaskEntity($"Task {i}", $"Description {i}"));
    }
} 