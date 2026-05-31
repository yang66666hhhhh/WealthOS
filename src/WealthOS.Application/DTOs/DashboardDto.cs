namespace WealthOS.Application.DTOs;

public record DashboardDto
{
    public decimal NetWorth { get; init; }
    public decimal NetWorthChange { get; init; }
    public decimal TotalAssets { get; init; }
    public decimal TotalLiabilities { get; init; }
    public decimal CashTotal { get; init; }
    public decimal InvestmentTotal { get; init; }
    public decimal SavingsRate { get; init; }
    public decimal DebtRatio { get; init; }
    public decimal MonthIncome { get; init; }
    public decimal MonthExpense { get; init; }
    public List<AssetAllocationDto> AssetAllocations { get; init; } = [];
    public List<NetWorthPointDto> NetWorthHistory { get; init; } = [];
    public List<TransactionDto> RecentTransactions { get; init; } = [];
}

public record AssetAllocationDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public decimal Percentage { get; init; }
    public string Color { get; init; } = string.Empty;
}

public record NetWorthPointDto
{
    public DateTime Date { get; init; }
    public decimal Value { get; init; }
}
