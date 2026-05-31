using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class ReportService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly INetWorthRepository _netWorthRepo;
    private readonly ICategoryRepository _categoryRepo;

    public ReportService(
        ITransactionRepository transactionRepo,
        INetWorthRepository netWorthRepo,
        ICategoryRepository categoryRepo)
    {
        _transactionRepo = transactionRepo;
        _netWorthRepo = netWorthRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task<AnnualReportDto> GenerateAnnualReportAsync(int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31, 23, 59, 59);

        var transactions = await _transactionRepo.GetByDateRangeAsync(startDate, endDate);
        var categories = (await _categoryRepo.GetAllAsync()).ToDictionary(c => c.Id);

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var monthlyBreakdown = transactions
            .GroupBy(t => new { t.OccurredAt.Year, t.OccurredAt.Month })
            .Select(g => new MonthlyBreakdownDto
            {
                Month = $"{g.Key.Month:D2}月",
                Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            })
            .OrderBy(m => m.Month)
            .ToList();

        var topExpenseCategories = transactions
            .Where(t => t.Type == TransactionType.Expense && t.CategoryId.HasValue)
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => new CategoryBreakdownDto
            {
                Category = categories.TryGetValue(g.Key, out var cat) ? cat.Name : "未分类",
                Amount = g.Sum(t => t.Amount),
                Percentage = totalExpense > 0 ? g.Sum(t => t.Amount) / totalExpense * 100 : 0
            })
            .OrderByDescending(c => c.Amount)
            .Take(5)
            .ToList();

        var topIncomeCategories = transactions
            .Where(t => t.Type == TransactionType.Income && t.CategoryId.HasValue)
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => new CategoryBreakdownDto
            {
                Category = categories.TryGetValue(g.Key, out var cat) ? cat.Name : "未分类",
                Amount = g.Sum(t => t.Amount),
                Percentage = totalIncome > 0 ? g.Sum(t => t.Amount) / totalIncome * 100 : 0
            })
            .OrderByDescending(c => c.Amount)
            .Take(5)
            .ToList();

        var snapshots = await _netWorthRepo.GetByDateRangeAsync(startDate.AddMonths(-1), endDate);
        var startSnapshot = snapshots.Where(s => s.SnapshotDate < startDate).OrderByDescending(s => s.SnapshotDate).FirstOrDefault();
        var endSnapshot = snapshots.Where(s => s.SnapshotDate <= endDate).OrderByDescending(s => s.SnapshotDate).FirstOrDefault();

        return new AnnualReportDto
        {
            Year = year,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            StartNetWorth = startSnapshot?.NetWorth ?? 0,
            EndNetWorth = endSnapshot?.NetWorth ?? 0,
            MonthlyBreakdown = monthlyBreakdown,
            TopExpenseCategories = topExpenseCategories,
            TopIncomeCategories = topIncomeCategories
        };
    }

    public async Task ExportTransactionsToCsvAsync(string filePath, DateTime start, DateTime end)
    {
        var transactions = await _transactionRepo.GetByDateRangeAsync(start, end);
        var lines = new List<string> { "日期,类型,金额,备注" };

        foreach (var t in transactions.OrderByDescending(x => x.OccurredAt))
        {
            var typeStr = t.Type switch
            {
                TransactionType.Income => "收入",
                TransactionType.Expense => "支出",
                TransactionType.Transfer => "转账",
                _ => "未知"
            };
            lines.Add($"{t.OccurredAt:yyyy-MM-dd},{typeStr},{t.Amount},{t.Note ?? ""}");
        }

        await System.IO.File.WriteAllLinesAsync(filePath, lines, System.Text.Encoding.UTF8);
    }
}
