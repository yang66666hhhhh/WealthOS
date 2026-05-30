using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Common;

namespace WealthOS.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IDbContext Context;
    protected abstract string TableName { get; }

    protected BaseRepository(IDbContext context)
    {
        Context = context;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        using var connection = Context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(
            $"SELECT * FROM {TableName} WHERE Id = @Id", new { Id = id.ToString() });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<T>($"SELECT * FROM {TableName} ORDER BY CreatedAt DESC");
    }

    public virtual async Task<Guid> AddAsync(T entity)
    {
        using var connection = Context.CreateConnection();
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id")
            .Select(p => p.Name);
        var columns = string.Join(", ", properties);
        var parameters = string.Join(", ", properties.Select(p => $"@{p}"));

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await connection.ExecuteAsync(
            $"INSERT INTO {TableName} (Id, {columns}) VALUES (@Id, {parameters})", entity);
        return entity.Id;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        using var connection = Context.CreateConnection();
        entity.UpdatedAt = DateTime.UtcNow;
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && p.Name != "CreatedAt")
            .Select(p => p.Name);
        var setClauses = string.Join(", ", properties.Select(p => $"{p} = @{p}"));

        var affected = await connection.ExecuteAsync(
            $"UPDATE {TableName} SET {setClauses} WHERE Id = @Id", entity);
        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = Context.CreateConnection();
        var affected = await connection.ExecuteAsync(
            $"DELETE FROM {TableName} WHERE Id = @Id", new { Id = id.ToString() });
        return affected > 0;
    }
}
