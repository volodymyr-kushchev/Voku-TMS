// © 2025 Volodymyr Kushchev. Use of this code is restricted to evaluation purposes only.
// Contact: volodymyr.kushchev@gmail.com

using TaskManagement.Domain.Common;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

public interface ITaskRepository
{
    Task<TaskEntity> AddAsync(TaskEntity task);
    Task<TaskEntity?> GetByIdAsync(int id);
    Task<IEnumerable<TaskEntity>> GetAllAsync();
    Task<PaginatedResult<TaskEntity>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalCountAsync();
    Task UpdateAsync(TaskEntity task);
    Task<bool> ExistsAsync(int id);
} 