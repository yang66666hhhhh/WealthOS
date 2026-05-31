using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class SeedDataService
{
    private readonly IAccountRepository _accountRepo;
    private readonly IAssetRepository _assetRepo;
    private readonly ILiabilityRepository _liabilityRepo;
    private readonly ITransactionRepository _transactionRepo;
    private readonly IGoalRepository _goalRepo;
    private readonly ICategoryRepository _categoryRepo;

    public SeedDataService(
        IAccountRepository accountRepo,
        IAssetRepository assetRepo,
        ILiabilityRepository liabilityRepo,
        ITransactionRepository transactionRepo,
        IGoalRepository goalRepo,
        ICategoryRepository categoryRepo)
    {
        _accountRepo = accountRepo;
        _assetRepo = assetRepo;
        _liabilityRepo = liabilityRepo;
        _transactionRepo = transactionRepo;
        _goalRepo = goalRepo;
        _categoryRepo = categoryRepo;
    }

    public async Task SeedAllAsync()
    {
        await SeedAccountsAsync();
        await SeedAssetsAsync();
        await SeedLiabilitiesAsync();
        await SeedGoalsAsync();
        await SeedTransactionsAsync();
    }

    private async Task SeedAccountsAsync()
    {
        var existing = await _accountRepo.GetAllAsync();
        if (existing.Any()) return;

        var accounts = new List<Account>
        {
            new() { Name = "招商银行储蓄卡", Type = AssetType.Bank, Balance = 85000, Institution = "招商银行", Note = "工资卡" },
            new() { Name = "工商银行储蓄卡", Type = AssetType.Bank, Balance = 32000, Institution = "工商银行", Note = "日常消费" },
            new() { Name = "支付宝余额", Type = AssetType.Cash, Balance = 12500, Institution = "支付宝" },
            new() { Name = "微信零钱", Type = AssetType.Cash, Balance = 3800, Institution = "微信" },
            new() { Name = "证券账户", Type = AssetType.Stock, Balance = 150000, Institution = "华泰证券", Note = "股票投资" },
        };

        foreach (var account in accounts)
        {
            await _accountRepo.AddAsync(account);
        }
    }

    private async Task SeedAssetsAsync()
    {
        var existing = await _assetRepo.GetAllAsync();
        if (existing.Any()) return;

        var assets = new List<Asset>
        {
            new() { Name = "沪深300ETF", Type = AssetType.ETF, CurrentValue = 85000, InitialValue = 80000, Institution = "华泰证券", Note = "长期持有" },
            new() { Name = "中证500ETF", Type = AssetType.ETF, CurrentValue = 45000, InitialValue = 50000, Institution = "华泰证券" },
            new() { Name = "贵州茅台", Type = AssetType.Stock, CurrentValue = 28000, InitialValue = 25000, Institution = "华泰证券" },
            new() { Name = "腾讯控股", Type = AssetType.Stock, CurrentValue = 18000, InitialValue = 22000, Institution = "华泰证券" },
            new() { Name = "黄金ETF", Type = AssetType.Gold, CurrentValue = 35000, InitialValue = 30000, Institution = "华泰证券" },
            new() { Name = "定期存款", Type = AssetType.Bank, CurrentValue = 100000, InitialValue = 100000, Institution = "招商银行", Note = "3年期" },
            new() { Name = "家用轿车", Type = AssetType.Car, CurrentValue = 120000, InitialValue = 180000, DepreciationRate = 15, PurchaseDate = new DateTime(2022, 6, 15) },
        };

        foreach (var asset in assets)
        {
            await _assetRepo.AddAsync(asset);
        }
    }

    private async Task SeedLiabilitiesAsync()
    {
        var existing = await _liabilityRepo.GetAllAsync();
        if (existing.Any()) return;

        var liabilities = new List<Liability>
        {
            new() { Name = "房贷", Type = LiabilityType.Mortgage, Balance = 850000, InterestRate = 3.85m, MonthlyPayment = 4500, StartDate = new DateTime(2020, 3, 1), Institution = "招商银行" },
            new() { Name = "车贷", Type = LiabilityType.CarLoan, Balance = 45000, InterestRate = 4.5m, MonthlyPayment = 2800, StartDate = new DateTime(2023, 1, 1), Institution = "工商银行" },
            new() { Name = "信用卡", Type = LiabilityType.CreditCard, Balance = 8500, InterestRate = 0, MonthlyPayment = 0, StartDate = DateTime.Now.AddMonths(-1), Institution = "招商银行" },
        };

        foreach (var liability in liabilities)
        {
            await _liabilityRepo.AddAsync(liability);
        }
    }

    private async Task SeedGoalsAsync()
    {
        var existing = await _goalRepo.GetAllAsync();
        if (existing.Any()) return;

        var goals = new List<Goal>
        {
            new() { Name = "应急基金", TargetAmount = 100000, CurrentAmount = 65000, TargetDate = new DateTime(2025, 12, 31), Icon = "🛡️", Note = "6个月生活费" },
            new() { Name = "旅行基金", TargetAmount = 30000, CurrentAmount = 12000, TargetDate = new DateTime(2025, 8, 1), Icon = "✈️", Note = "日本旅行" },
            new() { Name = "子女教育", TargetAmount = 500000, CurrentAmount = 85000, TargetDate = new DateTime(2030, 9, 1), Icon = "🎓" },
            new() { Name = "提前还房贷", TargetAmount = 200000, CurrentAmount = 45000, TargetDate = new DateTime(2026, 6, 30), Icon = "🏠" },
        };

        foreach (var goal in goals)
        {
            await _goalRepo.AddAsync(goal);
        }
    }

    private async Task SeedTransactionsAsync()
    {
        var existing = await _transactionRepo.GetAllAsync();
        if (existing.Any()) return;

        var accounts = (await _accountRepo.GetAllAsync()).ToList();
        var categories = (await _categoryRepo.GetAllAsync()).ToList();

        if (!accounts.Any() || !categories.Any()) return;

        var salaryAccount = accounts.First(a => a.Name.Contains("招商银行"));
        var consumeAccount = accounts.First(a => a.Name.Contains("工商银行"));

        var incomeCategories = categories.Where(c => c.Type == TransactionType.Income).ToList();
        var expenseCategories = categories.Where(c => c.Type == TransactionType.Expense).ToList();

        var transactions = new List<Transaction>
        {
            // 本月收入
            new() { Type = TransactionType.Income, Amount = 25000, AccountId = salaryAccount.Id, CategoryId = incomeCategories.First(c => c.Name == "工资").Id, Note = "12月工资", OccurredAt = DateTime.Now.AddDays(-5) },
            new() { Type = TransactionType.Income, Amount = 5000, AccountId = salaryAccount.Id, CategoryId = incomeCategories.First(c => c.Name == "奖金").Id, Note = "年终奖预发", OccurredAt = DateTime.Now.AddDays(-3) },
            new() { Type = TransactionType.Income, Amount = 2800, AccountId = consumeAccount.Id, CategoryId = incomeCategories.First(c => c.Name == "投资收益").Id, Note = "ETF分红", OccurredAt = DateTime.Now.AddDays(-10) },

            // 本月支出
            new() { Type = TransactionType.Expense, Amount = 3500, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "餐饮").Id, Note = "日常餐饮", OccurredAt = DateTime.Now.AddDays(-1) },
            new() { Type = TransactionType.Expense, Amount = 800, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "交通").Id, Note = "加油+停车", OccurredAt = DateTime.Now.AddDays(-2) },
            new() { Type = TransactionType.Expense, Amount = 2200, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "购物").Id, Note = "日用品采购", OccurredAt = DateTime.Now.AddDays(-4) },
            new() { Type = TransactionType.Expense, Amount = 4500, AccountId = salaryAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "住房").Id, Note = "房贷月供", OccurredAt = DateTime.Now.AddDays(-6) },
            new() { Type = TransactionType.Expense, Amount = 2800, AccountId = salaryAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "住房").Id, Note = "车贷月供", OccurredAt = DateTime.Now.AddDays(-6) },
            new() { Type = TransactionType.Expense, Amount = 500, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "娱乐").Id, Note = "电影+游戏", OccurredAt = DateTime.Now.AddDays(-7) },
            new() { Type = TransactionType.Expense, Amount = 1200, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "医疗").Id, Note = "体检费用", OccurredAt = DateTime.Now.AddDays(-8) },
            new() { Type = TransactionType.Expense, Amount = 350, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "通讯").Id, Note = "手机话费", OccurredAt = DateTime.Now.AddDays(-9) },
            new() { Type = TransactionType.Expense, Amount = 280, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "水电煤").Id, Note = "水电费", OccurredAt = DateTime.Now.AddDays(-11) },

            // 上月交易
            new() { Type = TransactionType.Income, Amount = 25000, AccountId = salaryAccount.Id, CategoryId = incomeCategories.First(c => c.Name == "工资").Id, Note = "11月工资", OccurredAt = DateTime.Now.AddMonths(-1).AddDays(-5) },
            new() { Type = TransactionType.Expense, Amount = 3800, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "餐饮").Id, Note = "日常餐饮", OccurredAt = DateTime.Now.AddMonths(-1).AddDays(-1) },
            new() { Type = TransactionType.Expense, Amount = 4500, AccountId = salaryAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "住房").Id, Note = "房贷月供", OccurredAt = DateTime.Now.AddMonths(-1).AddDays(-6) },
            new() { Type = TransactionType.Expense, Amount = 1500, AccountId = consumeAccount.Id, CategoryId = expenseCategories.First(c => c.Name == "购物").Id, Note = "双十一购物", OccurredAt = DateTime.Now.AddMonths(-1).AddDays(-10) },
            new() { Type = TransactionType.Income, Amount = 8000, AccountId = salaryAccount.Id, CategoryId = incomeCategories.First(c => c.Name == "兼职").Id, Note = "项目外包", OccurredAt = DateTime.Now.AddMonths(-1).AddDays(-15) },
        };

        foreach (var transaction in transactions)
        {
            await _transactionRepo.AddAsync(transaction);
        }
    }
}
