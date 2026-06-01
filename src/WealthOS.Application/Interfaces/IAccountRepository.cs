using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<IEnumerable<Account>> GetByTypeAsync(AccountType type);
    Task<decimal> GetTotalBalanceAsync();
}
