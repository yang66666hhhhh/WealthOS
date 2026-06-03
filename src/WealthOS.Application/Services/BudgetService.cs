using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class BudgetService
{
    private readonly IBudgetRepository _repo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly ICategoryRepository _categoryRepo;

    public BudgetService(IBudgetRepository repo, ITransactionRepository transactionRepo, ICategoryRepository categoryRepo)
    {
        _repo = repo;
        _transactionRepo = transactionRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<IEnumerable<BudgetDto>> GetBudgetsAsync(int year, int month)
    {
        var budgets = await _repo.GetByMonthAsync(year, month);
        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var transactions = await _transactionRepo.GetByDateRangeAsync(monthStart, monthEnd);
        var categories = (await _categoryRepo.GetAllAsync()).ToList();

        var expenseByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense && t.CategoryId.HasValue)
            .GroupBy(t => t.CategoryId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        return budgets.Select(b =>
        {
            decimal spent = b.Spent;
            if (Guid.TryParse(b.CategoryId, out var catId) && expenseByCategory.TryGetValue(catId, out var calculated))
                spent = calculated;

            return new BudgetDto
            {
                Id = b.Id,
                Name = b.Name,
                Amount = b.Amount,
                Spent = spent,
                Month = b.Month,
                Year = b.Year,
                Note = b.Note
            };
        });
    }

    public async Task<Guid> AddBudgetAsync(Budget budget) => await _repo.AddAsync(budget);
    public async Task<bool> UpdateBudgetAsync(Budget budget) => await _repo.UpdateAsync(budget);
    public async Task<bool> DeleteBudgetAsync(Guid id) => await _repo.DeleteAsync(id);
    public async Task<Budget?> GetBudgetAsync(Guid id) => await _repo.GetByIdAsync(id);
}
