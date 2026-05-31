using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Infrastructure.Repositories;

public class InvestmentHoldingRepository : BaseRepository<InvestmentHolding>, IInvestmentHoldingRepository
{
    protected override string TableName => "InvestmentHoldings";

    public InvestmentHoldingRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<InvestmentHolding>> GetByAssetTypeAsync(AssetType type)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<InvestmentHolding>(
            "SELECT * FROM InvestmentHoldings WHERE AssetType = @Type", new { Type = (int)type });
    }
}
