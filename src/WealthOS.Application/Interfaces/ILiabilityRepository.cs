using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Interfaces;

public interface ILiabilityRepository : IRepository<Liability>
{
    Task<IEnumerable<Liability>> GetByTypeAsync(LiabilityType type);
    Task<decimal> GetTotalBalanceAsync();
}
