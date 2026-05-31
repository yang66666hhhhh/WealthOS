using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class CategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Category>> GetAllAsync() => await _repo.GetAllAsync();
    public async Task<IEnumerable<Category>> GetByTypeAsync(TransactionType type) => await _repo.GetByTypeAsync(type);

    public async Task SeedDefaultCategoriesAsync()
    {
        var existing = await _repo.GetAllAsync();
        if (existing.Any()) return;

        var defaults = new List<Category>
        {
            new() { Name = "工资", Type = TransactionType.Income, Icon = "💰", SortOrder = 1 },
            new() { Name = "奖金", Type = TransactionType.Income, Icon = "🎁", SortOrder = 2 },
            new() { Name = "投资收益", Type = TransactionType.Income, Icon = "📈", SortOrder = 3 },
            new() { Name = "兼职", Type = TransactionType.Income, Icon = "💼", SortOrder = 4 },
            new() { Name = "红包", Type = TransactionType.Income, Icon = "🧧", SortOrder = 5 },
            new() { Name = "其他收入", Type = TransactionType.Income, Icon = "📦", SortOrder = 6 },
            new() { Name = "餐饮", Type = TransactionType.Expense, Icon = "🍜", SortOrder = 1 },
            new() { Name = "交通", Type = TransactionType.Expense, Icon = "🚗", SortOrder = 2 },
            new() { Name = "购物", Type = TransactionType.Expense, Icon = "🛒", SortOrder = 3 },
            new() { Name = "住房", Type = TransactionType.Expense, Icon = "🏠", SortOrder = 4 },
            new() { Name = "娱乐", Type = TransactionType.Expense, Icon = "🎮", SortOrder = 5 },
            new() { Name = "医疗", Type = TransactionType.Expense, Icon = "🏥", SortOrder = 6 },
            new() { Name = "教育", Type = TransactionType.Expense, Icon = "📚", SortOrder = 7 },
            new() { Name = "通讯", Type = TransactionType.Expense, Icon = "📱", SortOrder = 8 },
            new() { Name = "水电煤", Type = TransactionType.Expense, Icon = "💡", SortOrder = 9 },
            new() { Name = "其他支出", Type = TransactionType.Expense, Icon = "📦", SortOrder = 10 },
        };

        foreach (var category in defaults)
        {
            await _repo.AddAsync(category);
        }
    }
}
