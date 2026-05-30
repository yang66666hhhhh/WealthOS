using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Infrastructure.Repositories;

public class GoalRepository : BaseRepository<Goal>, IGoalRepository
{
    protected override string TableName => "Goals";

    public GoalRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<Goal>> GetByStatusAsync(GoalStatus status)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Goal>(
            "SELECT * FROM Goals WHERE Status = @Status", new { Status = (int)status });
    }
}
