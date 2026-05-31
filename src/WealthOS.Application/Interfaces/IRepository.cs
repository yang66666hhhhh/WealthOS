using System.Data;
using WealthOS.Domain.Common;

namespace WealthOS.Application.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<Guid> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);

    Task<T?> GetByIdAsync(Guid id, IDbTransaction transaction);
    Task<Guid> AddAsync(T entity, IDbTransaction transaction);
    Task<bool> UpdateAsync(T entity, IDbTransaction transaction);
    Task<bool> DeleteAsync(Guid id, IDbTransaction transaction);
}
