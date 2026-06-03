using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class DashboardService
{
    private readonly IAccountRepository _accountRepo;
    private readonly IAssetRepository _assetRepo;
    private readonly ILiabilityRepository _liabilityRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly INetWorthRepository _netWorthRepo;

    public DashboardService(
        IAccountRepository accountRepo,
        IAssetRepository assetRepo,
        ILiabilityRepository liabilityRepo,
        ITransactionRepository transactionRepo,
        INetWorthRepository netWorthRepo)
    {
        _accountRepo = accountRepo;
        _assetRepo = assetRepo;
        _liabilityRepo = liabilityRepo;
        _transactionRepo = transactionRepo;
        _netWorthRepo = netWorthRepo;
    }

    public async Task<DashboardDto> GetDashboardAsync(int historyDays = 90)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var prevMonthStart = monthStart.AddMonths(-1);
        var historyStart = now.AddDays(-historyDays);

        var totalAssetsTask = _assetRepo.GetTotalValueAsync();
        var totalLiabilitiesTask = _liabilityRepo.GetTotalBalanceAsync();
        var accountsTask = _accountRepo.GetAllAsync();
        var monthIncomeTask = _transactionRepo.GetTotalByTypeAsync(TransactionType.Income, monthStart, now);
        var monthExpenseTask = _transactionRepo.GetTotalByTypeAsync(TransactionType.Expense, monthStart, now);
        var prevSnapshotTask = _netWorthRepo.GetByDateRangeAsync(prevMonthStart, monthStart);
        var assetsTask = _assetRepo.GetAllAsync();
        var snapshotsTask = _netWorthRepo.GetByDateRangeAsync(historyStart, now);
        var recentTransactionsTask = _transactionRepo.GetByDateRangeAsync(now.AddDays(-30), now);

        await Task.WhenAll(
            totalAssetsTask, totalLiabilitiesTask, accountsTask,
            monthIncomeTask, monthExpenseTask,
            prevSnapshotTask, assetsTask, snapshotsTask, recentTransactionsTask);

        var totalAssets = totalAssetsTask.Result;
        var totalLiabilities = totalLiabilitiesTask.Result;
        var netWorth = totalAssets - totalLiabilities;
        var accounts = accountsTask.Result;
        var monthIncome = monthIncomeTask.Result;
        var monthExpense = monthExpenseTask.Result;

        var cashTotal = accounts
            .Where(a => a.Type is AccountType.Cash or AccountType.Bank)
            .Sum(a => a.Balance);
        var investmentTotal = accounts
            .Where(a => a.Type is AccountType.Investment)
            .Sum(a => a.Balance);

        var savingsRate = monthIncome > 0 ? (monthIncome - monthExpense) / monthIncome * 100 : 0;
        var debtRatio = totalAssets > 0 ? totalLiabilities / totalAssets * 100 : 0;

        var prevSnapshot = prevSnapshotTask.Result
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefault();
        var netWorthChange = prevSnapshot != null ? netWorth - prevSnapshot.NetWorth : 0;

        var assets = assetsTask.Result.Where(a => a.IsActive);
        var assetAllocations = assets
            .GroupBy(a => a.Type)
            .Select(g => new AssetAllocationDto
            {
                Name = g.Key.ToString(),
                Value = g.Sum(a => a.CurrentValue),
                Percentage = totalAssets > 0 ? g.Sum(a => a.CurrentValue) / totalAssets * 100 : 0
            })
            .OrderByDescending(a => a.Value)
            .ToList();

        var netWorthHistory = snapshotsTask.Result
            .OrderBy(s => s.SnapshotDate)
            .Select(s => new NetWorthPointDto { Date = s.SnapshotDate, Value = s.NetWorth })
            .ToList();

        if (netWorthHistory.Count == 0)
        {
            netWorthHistory.Add(new NetWorthPointDto { Date = now, Value = netWorth });
        }

        var accountLookup = accounts.ToDictionary(a => a.Id);
        var recentTransactions = recentTransactionsTask.Result
            .OrderByDescending(t => t.OccurredAt)
            .Take(10)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type,
                Amount = t.Amount,
                Note = t.Note,
                OccurredAt = t.OccurredAt,
                AccountName = accountLookup.TryGetValue(t.AccountId, out var acc) ? acc.Name : ""
            })
            .ToList();

        return new DashboardDto
        {
            NetWorth = netWorth,
            NetWorthChange = netWorthChange,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            CashTotal = cashTotal,
            InvestmentTotal = investmentTotal,
            SavingsRate = savingsRate,
            DebtRatio = debtRatio,
            MonthIncome = monthIncome,
            MonthExpense = monthExpense,
            AssetAllocations = assetAllocations,
            NetWorthHistory = netWorthHistory,
            RecentTransactions = recentTransactions
        };
    }
}
