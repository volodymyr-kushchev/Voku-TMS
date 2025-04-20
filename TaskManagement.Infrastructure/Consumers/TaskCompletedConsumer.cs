using MassTransit;
using Microsoft.Extensions.Logging;
using TaskManagement.Domain.Contracts;

namespace TaskManagement.Infrastructure.Consumers;

public class TaskCompletedConsumer : IConsumer<TaskCompletedEvent>
{
    private readonly ILogger<TaskCompletedConsumer> _logger;

    public TaskCompletedConsumer(ILogger<TaskCompletedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<TaskCompletedEvent> context)
    {
        _logger.LogInformation(
            "Task completed: {TaskId} - {TaskName} at {CompletedAt}",
            context.Message.TaskId,
            context.Message.TaskName,
            context.Message.CompletedAt);

        return Task.CompletedTask;
    }
} 