using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Infrastructure.Repositories;

public class AssetRepository : BaseRepository<Asset>, IAssetRepository
{
    protected override string TableName => "Assets";

    public AssetRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<Asset>> GetByTypeAsync(AssetType type)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Asset>(
            "SELECT * FROM Assets WHERE Type = @Type AND IsActive = 1", new { Type = (int)type });
    }

    public async Task<decimal> GetTotalValueAsync()
    {
        using var connection = Context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(
            "SELECT COALESCE(SUM(CurrentValue), 0) FROM Assets WHERE IsActive = 1");
    }
}
