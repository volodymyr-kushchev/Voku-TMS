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
    public async Task UpdateTaskStatus_ValidTransition_ReturnsOk()
    {
        var task = new TaskEntity
        {
            Name = "Test Task",
            Description = "Test Description",
            Status = TEStatus.NotStarted
        };

        _dbContext!.Tasks.Add(task);
        await _dbContext.SaveChangesAsync();

        var request = new { Status = TEStatus.InProgress };

        var response = await _client!.PutAsJsonAsync($"/api/tasks/{task.Id}/status", request);

        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal(TEStatus.InProgress, updatedTask.Status);
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