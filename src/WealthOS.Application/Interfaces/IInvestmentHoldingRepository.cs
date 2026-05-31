using WealthOS.Domain.Entities;

namespace WealthOS.Application.Interfaces;

public interface IInvestmentHoldingRepository : IRepository<InvestmentHolding>
{
    Task<IEnumerable<InvestmentHolding>> GetByAssetTypeAsync(Domain.Enums.AssetType type);
}
