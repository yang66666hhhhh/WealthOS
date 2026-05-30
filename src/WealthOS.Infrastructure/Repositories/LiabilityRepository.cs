using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Infrastructure.Repositories;

public class LiabilityRepository : BaseRepository<Liability>, ILiabilityRepository
{
    protected override string TableName => "Liabilities";

    public LiabilityRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<Liability>> GetByTypeAsync(LiabilityType type)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Liability>(
            "SELECT * FROM Liabilities WHERE Type = @Type AND IsActive = 1", new { Type = (int)type });
    }

    public async Task<decimal> GetTotalBalanceAsync()
    {
        using var connection = Context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(
            "SELECT COALESCE(SUM(Balance), 0) FROM Liabilities WHERE IsActive = 1");
    }
}
