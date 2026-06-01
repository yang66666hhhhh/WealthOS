using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
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
                Month = $"{g.Key.Month:D2}",
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
                Category = categories.TryGetValue(g.Key, out var cat) ? cat.Name : "Uncategorized",
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
                Category = categories.TryGetValue(g.Key, out var cat) ? cat.Name : "Uncategorized",
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
        var lines = new List<string> { "Date,Type,Amount,Note" };

        foreach (var t in transactions.OrderByDescending(x => x.OccurredAt))
        {
            var typeStr = t.Type switch
            {
                TransactionType.Income => "Income",
                TransactionType.Expense => "Expense",
                TransactionType.Transfer => "Transfer",
                _ => "Unknown"
            };
            lines.Add($"{t.OccurredAt:yyyy-MM-dd},{typeStr},{t.Amount},\"{t.Note ?? ""}\"");
        }

        await System.IO.File.WriteAllLinesAsync(filePath, lines, System.Text.Encoding.UTF8);
    }

    public async Task ExportAnnualReportToPdfAsync(string filePath, int year, string currencySymbol = "￥")
    {
        var report = await GenerateAnnualReportAsync(year);
        var netWorthChange = report.EndNetWorth - report.StartNetWorth;
        var savingsRate = report.TotalIncome > 0 ? (report.TotalIncome - report.TotalExpense) / report.TotalIncome * 100 : 0;

        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text($"{year} Annual Wealth Report")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().Text("Financial Summary").SemiBold().FontSize(14);
                    col.Item().PaddingTop(10).Grid(grid =>
                    {
                        grid.Columns(2);
                        grid.Item().Text($"Total Income: {currencySymbol}{report.TotalIncome:N2}");
                        grid.Item().Text($"Total Expense: {currencySymbol}{report.TotalExpense:N2}");
                        grid.Item().Text($"Savings Rate: {savingsRate:F1}%");
                        grid.Item().Text($"Net Income: {currencySymbol}{report.TotalIncome - report.TotalExpense:N2}");
                    });

                    col.Item().PaddingTop(20).Text("Net Worth").SemiBold().FontSize(14);
                    col.Item().PaddingTop(10).Grid(grid =>
                    {
                        grid.Columns(2);
                        grid.Item().Text($"Start of Year: {currencySymbol}{report.StartNetWorth:N2}");
                        grid.Item().Text($"End of Year: {currencySymbol}{report.EndNetWorth:N2}");
                        grid.Item().Text($"Change: {currencySymbol}{netWorthChange:N2} ({(report.StartNetWorth > 0 ? netWorthChange / report.StartNetWorth * 100 : 0):F1}%)");
                    });

                    if (report.MonthlyBreakdown.Any())
                    {
                        col.Item().PaddingTop(20).Text("Monthly Breakdown").SemiBold().FontSize(14);
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(60);
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Month").SemiBold();
                                header.Cell().AlignRight().Text("Income").SemiBold();
                                header.Cell().AlignRight().Text("Expense").SemiBold();
                                header.Cell().AlignRight().Text("Net").SemiBold();
                            });

                            foreach (var m in report.MonthlyBreakdown)
                            {
                                table.Cell().Text(m.Month);
                                table.Cell().AlignRight().Text($"{currencySymbol}{m.Income:N0}");
                                table.Cell().AlignRight().Text($"{currencySymbol}{m.Expense:N0}");
                                table.Cell().AlignRight().Text($"{currencySymbol}{m.Income - m.Expense:N0}");
                            }
                        });
                    }

                    if (report.TopExpenseCategories.Any())
                    {
                        col.Item().PaddingTop(20).Text("Top Expense Categories").SemiBold().FontSize(14);
                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Category").SemiBold();
                                header.Cell().AlignRight().Text("Amount").SemiBold();
                                header.Cell().AlignRight().Text("%").SemiBold();
                            });

                            foreach (var c in report.TopExpenseCategories)
                            {
                                table.Cell().Text(c.Category);
                                table.Cell().AlignRight().Text($"{currencySymbol}{c.Amount:N0}");
                                table.Cell().AlignRight().Text($"{c.Percentage:F1}%");
                            }
                        });
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"Generated by WealthOS on {DateTime.Now:yyyy-MM-dd HH:mm}")
                    .FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf(filePath);
    }
}
