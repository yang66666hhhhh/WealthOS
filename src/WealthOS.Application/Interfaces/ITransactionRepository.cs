using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Transaction>> GetByAccountAsync(Guid accountId);
    Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type);
    Task<decimal> GetTotalByTypeAsync(TransactionType type, DateTime start, DateTime end);
}
