using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;

namespace WealthOS.Infrastructure.Repositories;

public class BudgetRepository : BaseRepository<Budget>, IBudgetRepository
{
    protected override string TableName => "Budgets";

    public BudgetRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<Budget>> GetByMonthAsync(int year, int month)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Budget>(
            "SELECT * FROM Budgets WHERE Year = @Year AND Month = @Month",
            new { Year = year, Month = month });
    }
}
