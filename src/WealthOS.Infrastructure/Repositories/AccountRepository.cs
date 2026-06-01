using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Infrastructure.Repositories;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    protected override string TableName => "Accounts";

    public AccountRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<Account>> GetByTypeAsync(AccountType type)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Account>(
            "SELECT * FROM Accounts WHERE Type = @Type AND IsActive = 1", new { Type = (int)type });
    }

    public async Task<decimal> GetTotalBalanceAsync()
    {
        using var connection = Context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(
            "SELECT COALESCE(SUM(Balance), 0) FROM Accounts WHERE IsActive = 1");
    }
}
