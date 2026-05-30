using WealthOS.Domain.Entities;

namespace WealthOS.Application.Interfaces;

public interface INetWorthRepository : IRepository<NetWorthSnapshot>
{
    Task<IEnumerable<NetWorthSnapshot>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<NetWorthSnapshot?> GetLatestAsync();
}
