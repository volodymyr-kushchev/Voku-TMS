// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using Microsoft.EntityFrameworkCore;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.UnitTests.Common;

public abstract class TestBase : IAsyncLifetime
{
    protected ApplicationDbContext Context { get; private set; }

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }

    protected async Task<T> AddEntityAsync<T>(T entity) where T : class
    {
        Context.Add(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    protected async Task<IEnumerable<T>> AddEntitiesAsync<T>(IEnumerable<T> entities) where T : class
    {
        Context.AddRange(entities);
        await Context.SaveChangesAsync();
        return entities;
    }
} 