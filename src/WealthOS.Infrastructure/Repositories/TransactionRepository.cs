using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Infrastructure.Repositories;

public class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    protected override string TableName => "Transactions";

    public TransactionRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Transaction>(
            "SELECT * FROM Transactions WHERE OccurredAt >= @Start AND OccurredAt <= @End ORDER BY OccurredAt DESC",
            new { Start = start.ToString("o"), End = end.ToString("o") });
    }

    public async Task<IEnumerable<Transaction>> GetByAccountAsync(Guid accountId)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Transaction>(
            "SELECT * FROM Transactions WHERE AccountId = @AccountId ORDER BY OccurredAt DESC",
            new { AccountId = accountId.ToString() });
    }

    public async Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Transaction>(
            "SELECT * FROM Transactions WHERE Type = @Type ORDER BY OccurredAt DESC",
            new { Type = (int)type });
    }

    public async Task<decimal> GetTotalByTypeAsync(TransactionType type, DateTime start, DateTime end)
    {
        using var connection = Context.CreateConnection();
        return await connection.ExecuteScalarAsync<decimal>(
            "SELECT COALESCE(SUM(Amount), 0) FROM Transactions WHERE Type = @Type AND OccurredAt >= @Start AND OccurredAt <= @End",
            new { Type = (int)type, Start = start.ToString("o"), End = end.ToString("o") });
    }
}
