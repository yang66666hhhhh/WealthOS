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
        var totalAssets = await _assetRepo.GetTotalValueAsync();
        var totalLiabilities = await _liabilityRepo.GetTotalBalanceAsync();
        var netWorth = totalAssets - totalLiabilities;

        var accounts = await _accountRepo.GetAllAsync();
        var cashTotal = accounts
            .Where(a => a.Type is AssetType.Cash or AssetType.Bank)
            .Sum(a => a.Balance);
        var investmentTotal = accounts
            .Where(a => a.Type is AssetType.Fund or AssetType.Stock or AssetType.ETF or AssetType.Gold or AssetType.Crypto)
            .Sum(a => a.Balance);

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthIncome = await _transactionRepo.GetTotalByTypeAsync(TransactionType.Income, monthStart, now);
        var monthExpense = await _transactionRepo.GetTotalByTypeAsync(TransactionType.Expense, monthStart, now);
        var savingsRate = monthIncome > 0 ? (monthIncome - monthExpense) / monthIncome * 100 : 0;
        var debtRatio = totalAssets > 0 ? totalLiabilities / totalAssets * 100 : 0;

        var prevMonthStart = monthStart.AddMonths(-1);
        var prevSnapshot = (await _netWorthRepo.GetByDateRangeAsync(prevMonthStart, monthStart))
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefault();
        var netWorthChange = prevSnapshot != null ? netWorth - prevSnapshot.NetWorth : 0;

        var assets = (await _assetRepo.GetAllAsync()).Where(a => a.IsActive);
        var assetAllocations = assets
            .GroupBy(a => a.Type)
            .Select(g => new AssetAllocationDto
            {
                Name = g.Key.ToString(),
                Value = g.Sum(a => a.CurrentValue),
                Percentage = totalAssets > 0 ? g.Sum(a => a.CurrentValue) / totalAssets * 100 : 0,
                Color = GetAssetColor(g.Key)
            })
            .OrderByDescending(a => a.Value)
            .ToList();

        var historyStart = now.AddDays(-historyDays);
        var snapshots = await _netWorthRepo.GetByDateRangeAsync(historyStart, now);
        var netWorthHistory = snapshots
            .OrderBy(s => s.SnapshotDate)
            .Select(s => new NetWorthPointDto { Date = s.SnapshotDate, Value = s.NetWorth })
            .ToList();

        if (netWorthHistory.Count == 0)
        {
            netWorthHistory.Add(new NetWorthPointDto { Date = now, Value = netWorth });
        }

        var recentTransactions = (await _transactionRepo.GetByDateRangeAsync(now.AddDays(-30), now))
            .OrderByDescending(t => t.OccurredAt)
            .Take(10)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type,
                Amount = t.Amount,
                Note = t.Note,
                OccurredAt = t.OccurredAt,
                AccountName = accounts.FirstOrDefault(a => a.Id == t.AccountId)?.Name ?? ""
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

    private static string GetAssetColor(AssetType type) => type switch
    {
        AssetType.Cash => "#4CAF50",
        AssetType.Bank => "#2196F3",
        AssetType.Fund => "#9C27B0",
        AssetType.Stock => "#FF9800",
        AssetType.ETF => "#00BCD4",
        AssetType.Crypto => "#F44336",
        AssetType.Gold => "#FFD700",
        AssetType.House => "#795548",
        AssetType.Car => "#607D8B",
        AssetType.Collection => "#E91E63",
        _ => "#9E9E9E"
    };
}
