// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Controller for managing tasks
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="request">The task creation request</param>
    /// <returns>The created task</returns>
    /// <response code="201">Returns the newly created task</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var task = await _taskService.CreateTaskAsync(request.Name, request.Description);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    /// <summary>
    /// Updates the status of a task
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <param name="request">The status update request</param>
    /// <returns>The updated task</returns>
    /// <response code="200">Returns the updated task</response>
    /// <response code="400">If the status transition is invalid</response>
    /// <response code="404">If the task is not found</response>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> PatchTaskStatus(int id, [FromBody] PatchTaskStatusRequest request)
    {
        var task = await _taskService.UpdateTaskStatusAsync(id, request.Status);
        return Ok(task);
    }

    /// <summary>
    /// Gets a paginated list of tasks
    /// </summary>
    /// <param name="request">The pagination request</param>
    /// <returns>A paginated list of tasks</returns>
    /// <response code="200">Returns the list of tasks</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<TaskDto>>> GetTasks([FromQuery] PaginationRequest? request = null)
    {
        var result = await _taskService.GetTasksAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Gets a task by ID
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <returns>The task</returns>
    /// <response code="200">Returns the task</response>
    /// <response code="404">If the task is not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> GetTask(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        return Ok(task);
    }
}