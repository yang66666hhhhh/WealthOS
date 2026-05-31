using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;

namespace WealthOS.Application.Services;

public class BudgetService
{
    private readonly IBudgetRepository _repo;

    public BudgetService(IBudgetRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<BudgetDto>> GetBudgetsAsync(int year, int month)
    {
        var budgets = await _repo.GetByMonthAsync(year, month);
        return budgets.Select(b => new BudgetDto
        {
            Id = b.Id,
            Name = b.Name,
            Amount = b.Amount,
            Spent = b.Spent,
            Month = b.Month,
            Year = b.Year,
            Note = b.Note
        });
    }

    public async Task<Guid> AddBudgetAsync(Budget budget) => await _repo.AddAsync(budget);
    public async Task<bool> UpdateBudgetAsync(Budget budget) => await _repo.UpdateAsync(budget);
    public async Task<bool> DeleteBudgetAsync(Guid id) => await _repo.DeleteAsync(id);
    public async Task<Budget?> GetBudgetAsync(Guid id) => await _repo.GetByIdAsync(id);
}
