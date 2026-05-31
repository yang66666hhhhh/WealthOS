using WealthOS.Domain.Entities;

namespace WealthOS.Application.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<IEnumerable<Budget>> GetByMonthAsync(int year, int month);
}
