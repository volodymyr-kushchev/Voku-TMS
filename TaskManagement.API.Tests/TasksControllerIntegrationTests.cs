// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using System.Net.Http.Json;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;
using TaskManagement.Infrastructure.Data;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace TaskManagement.API.Tests;

public class TasksControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private const string TestDatabaseName = "TaskManagementTestDb";
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient? _client;
    private IServiceScope? _scope;
    private ApplicationDbContext? _dbContext;
    private readonly MsSqlContainer _dbContainer;
    private readonly RabbitMqContainer _rabbitMqContainer;

    public TasksControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;

        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/azure-sql-edge")
            .WithPassword("Your_password123")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StartAsync(),
            _rabbitMqContainer.StartAsync()
        );

        var connectionString = $"{_dbContainer.GetConnectionString()};Initial Catalog={TestDatabaseName}";

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
            });
        });

        _client = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task CreateTask_ValidData_ReturnsCreated()
    {
        var request = new { Name = "Test Task", Description = "Test Description" };

        var response = await _client!.PostAsJsonAsync("/api/v1/tasks", request);

        response.EnsureSuccessStatusCode();
        var createdTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(createdTask);
        Assert.Equal(request.Name, createdTask.Name);
        Assert.Equal(request.Description, createdTask.Description);
        Assert.Equal(TEStatus.NotStarted, createdTask.Status);
    }

    [Fact]
    public async Task UpdateTaskStatus_ValidTransition_ReturnsOk()
    {
        var task = new TaskEntity("Test Task", "Test Description");

        _dbContext!.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var request = new { Status = TEStatus.InProgress };

        var response = await _client!.PatchAsJsonAsync($"/api/v1/tasks/{task.Id}/status", request);

        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal(TEStatus.InProgress, updatedTask.Status);
    }

    [Fact]
    public async Task GetTasks_WithPagination_ReturnsPaginatedResults()
    {
        var tasks = new List<TaskEntity>
        {
            new("Task 1", "Description 1"),
            new("Task 2", "Description 2"),
            new("Task 3", "Description 3")
        };

        _dbContext!.Tasks.AddRange(tasks);
        await _dbContext.SaveChangesAsync();

        var response = await _client!.GetAsync("/api/v1/tasks?pageNumber=1&pageSize=2");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<TaskDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
    }

    public async Task DisposeAsync()
    {
        await _dbContext!.Database.EnsureDeletedAsync();
        _scope!.Dispose();
        _client!.Dispose();

        await Task.WhenAll(
            _dbContainer.DisposeAsync().AsTask(),
            _rabbitMqContainer.DisposeAsync().AsTask()
        );
    }
}