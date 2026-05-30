using Dapper;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Infrastructure.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    protected override string TableName => "Categories";

    public CategoryRepository(IDbContext context) : base(context) { }

    public async Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type)
    {
        using var connection = Context.CreateConnection();
        return await connection.QueryAsync<Category>(
            "SELECT * FROM Categories WHERE Type = @Type ORDER BY SortOrder", new { Type = (int)type });
    }
}
