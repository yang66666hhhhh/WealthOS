namespace WealthOS.Application.DTOs;

public record BudgetDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal Spent { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Note { get; init; }
    public decimal Remaining => Amount - Spent;
    public decimal Progress => Amount > 0 ? Spent / Amount * 100 : 0;
    public bool IsOverBudget => Spent > Amount;
}
