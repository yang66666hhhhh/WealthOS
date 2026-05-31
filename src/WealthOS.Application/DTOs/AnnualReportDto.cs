using WealthOS.Application.Interfaces;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.DTOs;

public record AnnualReportDto
{
    public int Year { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal NetSavings => TotalIncome - TotalExpense;
    public decimal SavingsRate => TotalIncome > 0 ? NetSavings / TotalIncome * 100 : 0;
    public decimal StartNetWorth { get; init; }
    public decimal EndNetWorth { get; init; }
    public decimal NetWorthGrowth => EndNetWorth - StartNetWorth;
    public decimal NetWorthGrowthPercentage => StartNetWorth > 0 ? NetWorthGrowth / StartNetWorth * 100 : 0;
    public List<MonthlyBreakdownDto> MonthlyBreakdown { get; init; } = [];
    public List<CategoryBreakdownDto> TopExpenseCategories { get; init; } = [];
    public List<CategoryBreakdownDto> TopIncomeCategories { get; init; } = [];
}

public record MonthlyBreakdownDto
{
    public string Month { get; init; } = string.Empty;
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Savings => Income - Expense;
}

public record CategoryBreakdownDto
{
    public string Category { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal Percentage { get; init; }
}
