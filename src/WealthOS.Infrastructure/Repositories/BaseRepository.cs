using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Common;

namespace WealthOS.Infrastructure.Repositories;

public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IDbContext Context;
    protected abstract string TableName { get; }

    private static readonly PropertyInfo[] AddProperties = typeof(T).GetProperties()
        .Where(p => p.Name != "Id")
        .ToArray();

    private static readonly PropertyInfo[] UpdateProperties = typeof(T).GetProperties()
        .Where(p => p.Name != "Id" && p.Name != "CreatedAt")
        .ToArray();

    private static readonly string AddColumns = string.Join(", ", AddProperties.Select(p => p.Name));
    private static readonly string AddParameters = string.Join(", ", AddProperties.Select(p => $"@{p.Name}"));
    private static readonly string UpdateSetClauses = string.Join(", ", UpdateProperties.Select(p => $"{p.Name} = @{p.Name}"));

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

        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await connection.ExecuteAsync(
            $"INSERT INTO {TableName} (Id, {AddColumns}) VALUES (@Id, {AddParameters})", entity);
        return entity.Id;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        using var connection = Context.CreateConnection();
        entity.UpdatedAt = DateTime.UtcNow;

        var affected = await connection.ExecuteAsync(
            $"UPDATE {TableName} SET {UpdateSetClauses} WHERE Id = @Id", entity);
        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = Context.CreateConnection();
        var affected = await connection.ExecuteAsync(
            $"DELETE FROM {TableName} WHERE Id = @Id", new { Id = id.ToString() });
        return affected > 0;
    }

    public virtual async Task<Guid> AddAsync(T entity, IDbTransaction transaction)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await transaction.Connection!.ExecuteAsync(
            $"INSERT INTO {TableName} (Id, {AddColumns}) VALUES (@Id, {AddParameters})", entity, transaction);
        return entity.Id;
    }

    public virtual async Task<bool> UpdateAsync(T entity, IDbTransaction transaction)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        var affected = await transaction.Connection!.ExecuteAsync(
            $"UPDATE {TableName} SET {UpdateSetClauses} WHERE Id = @Id", entity, transaction);
        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id, IDbTransaction transaction)
    {
        var affected = await transaction.Connection!.ExecuteAsync(
            $"DELETE FROM {TableName} WHERE Id = @Id", new { Id = id.ToString() }, transaction);
        return affected > 0;
    }
}
