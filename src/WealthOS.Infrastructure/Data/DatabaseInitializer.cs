using WealthOS.Application.Interfaces;

namespace WealthOS.Infrastructure.Data;

public class DatabaseInitializer
{
    private readonly IDbContext _context;

    public DatabaseInitializer(IDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        using var connection = _context.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Accounts (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Balance REAL NOT NULL DEFAULT 0,
                Currency TEXT NOT NULL DEFAULT 'CNY',
                Institution TEXT NOT NULL DEFAULT '',
                Note TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Assets (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                CurrentValue REAL NOT NULL DEFAULT 0,
                InitialValue REAL NOT NULL DEFAULT 0,
                Currency TEXT NOT NULL DEFAULT 'CNY',
                Institution TEXT NOT NULL DEFAULT '',
                Note TEXT,
                DepreciationRate REAL,
                PurchaseDate TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Liabilities (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Balance REAL NOT NULL DEFAULT 0,
                InterestRate REAL NOT NULL DEFAULT 0,
                MonthlyPayment REAL NOT NULL DEFAULT 0,
                StartDate TEXT NOT NULL,
                EndDate TEXT,
                Currency TEXT NOT NULL DEFAULT 'CNY',
                Institution TEXT,
                Note TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Categories (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Icon TEXT,
                Color TEXT,
                SortOrder INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Transactions (
                Id TEXT PRIMARY KEY,
                Type INTEGER NOT NULL,
                Amount REAL NOT NULL,
                CategoryId TEXT,
                AccountId TEXT NOT NULL,
                ToAccountId TEXT,
                Note TEXT,
                OccurredAt TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL,
                FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                FOREIGN KEY (AccountId) REFERENCES Accounts(Id),
                FOREIGN KEY (ToAccountId) REFERENCES Accounts(Id)
            );

            CREATE TABLE IF NOT EXISTS Goals (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                TargetAmount REAL NOT NULL,
                CurrentAmount REAL NOT NULL DEFAULT 0,
                TargetDate TEXT NOT NULL,
                Status INTEGER NOT NULL DEFAULT 0,
                Icon TEXT,
                Note TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS InvestmentHoldings (
                Id TEXT PRIMARY KEY,
                Symbol TEXT NOT NULL,
                Name TEXT NOT NULL,
                AssetType INTEGER NOT NULL,
                Quantity REAL NOT NULL,
                AverageCost REAL NOT NULL,
                CurrentPrice REAL NOT NULL,
                Currency TEXT NOT NULL DEFAULT 'CNY',
                AccountId TEXT,
                Note TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS NetWorthSnapshots (
                Id TEXT PRIMARY KEY,
                TotalAssets REAL NOT NULL,
                TotalLiabilities REAL NOT NULL,
                NetWorth REAL NOT NULL,
                SnapshotDate TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_transactions_occurred ON Transactions(OccurredAt);
            CREATE INDEX IF NOT EXISTS idx_transactions_account ON Transactions(AccountId);
            CREATE INDEX IF NOT EXISTS idx_transactions_type ON Transactions(Type);
            CREATE INDEX IF NOT EXISTS idx_networth_date ON NetWorthSnapshots(SnapshotDate);
        ";

        await Task.Run(() => command.ExecuteNonQuery());
    }
}
