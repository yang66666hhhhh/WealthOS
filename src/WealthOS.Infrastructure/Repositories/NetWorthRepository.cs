using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;

namespace WealthOS.Infrastructure.Repositories;

public class NetWorthRepository : BaseRepository<NetWorthSnapshot>, INetWorthRepository
{
    protected override string TableName => "NetWorthSnapshots";

    public NetWorthRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<NetWorthSnapshot>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<NetWorthSnapshot>(
            "SELECT * FROM NetWorthSnapshots WHERE SnapshotDate >= @Start AND SnapshotDate <= @End ORDER BY SnapshotDate",
            new { Start = start.ToString("o"), End = end.ToString("o") });
    }

    public async Task<NetWorthSnapshot?> GetLatestAsync()
    {
        using var connection = Context.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<NetWorthSnapshot>(
            "SELECT * FROM NetWorthSnapshots ORDER BY SnapshotDate DESC LIMIT 1");
    }
}
