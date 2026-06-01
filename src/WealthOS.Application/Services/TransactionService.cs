using System.Data;
using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Common;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly NetWorthService _netWorthService;
    private readonly IDbContext _dbContext;

    public TransactionService(
        ITransactionRepository transactionRepo,
        IAccountRepository accountRepo,
        ICategoryRepository categoryRepo,
        NetWorthService netWorthService,
        IDbContext dbContext)
    {
        _transactionRepo = transactionRepo;
        _accountRepo = accountRepo;
        _categoryRepo = categoryRepo;
        _netWorthService = netWorthService;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(DateTime start, DateTime end)
    {
        var transactions = await _transactionRepo.GetByDateRangeAsync(start, end);
        var accounts = (await _accountRepo.GetAllAsync()).ToDictionary(a => a.Id);
        var categories = (await _categoryRepo.GetAllAsync()).ToDictionary(c => c.Id);

        return transactions
            .OrderByDescending(t => t.OccurredAt)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type,
                Amount = t.Amount,
                AccountId = t.AccountId,
                CategoryId = t.CategoryId,
                CategoryName = t.CategoryId.HasValue && categories.TryGetValue(t.CategoryId.Value, out var cat) ? cat.Name : null,
                AccountName = accounts.TryGetValue(t.AccountId, out var acc) ? acc.Name : "",
                Note = t.Note,
                OccurredAt = t.OccurredAt
            });
    }

    public async Task<Guid> AddTransactionAsync(Transaction transaction)
    {
        using var transaction_scope = _dbContext.BeginTransaction();

        try
        {
            var account = await _accountRepo.GetByIdAsync(transaction.AccountId, transaction_scope)
                ?? throw new InvalidOperationException($"Account {transaction.AccountId} not found.");

            var id = await _transactionRepo.AddAsync(transaction, transaction_scope);

            account.Balance += transaction.GetBalanceImpact();
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.UpdateAsync(account, transaction_scope);

            if (transaction.Type == TransactionType.Transfer && transaction.ToAccountId.HasValue)
            {
                var toAccount = await _accountRepo.GetByIdAsync(transaction.ToAccountId.Value, transaction_scope);
                if (toAccount != null)
                {
                    toAccount.Balance += transaction.Amount;
                    toAccount.UpdatedAt = DateTime.UtcNow;
                    await _accountRepo.UpdateAsync(toAccount, transaction_scope);
                }
            }

            transaction_scope.Commit();

            await _netWorthService.SaveSnapshotIfNotExistsTodayAsync();

            return id;
        }
        catch
        {
            transaction_scope.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteTransactionAsync(Guid id)
    {
        using var transaction_scope = _dbContext.BeginTransaction();

        try
        {
            var transaction = await _transactionRepo.GetByIdAsync(id, transaction_scope);
            if (transaction == null) return false;

            var account = await _accountRepo.GetByIdAsync(transaction.AccountId, transaction_scope);
            if (account != null)
            {
                account.Balance -= transaction.GetBalanceImpact();
                account.UpdatedAt = DateTime.UtcNow;
                await _accountRepo.UpdateAsync(account, transaction_scope);
            }

            if (transaction.Type == TransactionType.Transfer && transaction.ToAccountId.HasValue)
            {
                var toAccount = await _accountRepo.GetByIdAsync(transaction.ToAccountId.Value, transaction_scope);
                if (toAccount != null)
                {
                    toAccount.Balance -= transaction.Amount;
                    toAccount.UpdatedAt = DateTime.UtcNow;
                    await _accountRepo.UpdateAsync(toAccount, transaction_scope);
                }
            }

            var result = await _transactionRepo.DeleteAsync(id, transaction_scope);

            transaction_scope.Commit();

            await _netWorthService.SaveSnapshotIfNotExistsTodayAsync();

            return result;
        }
        catch
        {
            transaction_scope.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateTransactionAsync(Guid id, Transaction updatedTransaction)
    {
        using var transaction_scope = _dbContext.BeginTransaction();

        try
        {
            var originalTransaction = await _transactionRepo.GetByIdAsync(id, transaction_scope);
            if (originalTransaction == null) return false;

            var account = await _accountRepo.GetByIdAsync(originalTransaction.AccountId, transaction_scope)
                ?? throw new InvalidOperationException($"Account {originalTransaction.AccountId} not found.");

            account.Balance -= originalTransaction.GetBalanceImpact();
            account.Balance += updatedTransaction.GetBalanceImpact();

            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.UpdateAsync(account, transaction_scope);

            if (originalTransaction.Type == TransactionType.Transfer && originalTransaction.ToAccountId.HasValue)
            {
                var oldToAccount = await _accountRepo.GetByIdAsync(originalTransaction.ToAccountId.Value, transaction_scope);
                if (oldToAccount != null)
                {
                    oldToAccount.Balance -= originalTransaction.Amount;
                    oldToAccount.UpdatedAt = DateTime.UtcNow;
                    await _accountRepo.UpdateAsync(oldToAccount, transaction_scope);
                }
            }

            if (updatedTransaction.Type == TransactionType.Transfer && updatedTransaction.ToAccountId.HasValue)
            {
                var newToAccount = await _accountRepo.GetByIdAsync(updatedTransaction.ToAccountId.Value, transaction_scope);
                if (newToAccount != null)
                {
                    newToAccount.Balance += updatedTransaction.Amount;
                    newToAccount.UpdatedAt = DateTime.UtcNow;
                    await _accountRepo.UpdateAsync(newToAccount, transaction_scope);
                }
            }

            updatedTransaction.Id = id;
            updatedTransaction.CreatedAt = originalTransaction.CreatedAt;
            updatedTransaction.UpdatedAt = DateTime.UtcNow;
            await _transactionRepo.UpdateAsync(updatedTransaction, transaction_scope);

            transaction_scope.Commit();

            await _netWorthService.SaveSnapshotIfNotExistsTodayAsync();

            return true;
        }
        catch
        {
            transaction_scope.Rollback();
            throw;
        }
    }

    public async Task<int> ImportTransactionsFromCsvAsync(string filePath, Guid accountId)
    {
        var lines = await System.IO.File.ReadAllLinesAsync(filePath, System.Text.Encoding.UTF8);
        var count = 0;

        foreach (var line in lines.Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length < 3) continue;

            var typeStr = parts[1].Trim().Trim('"').ToLower();
            var type = typeStr switch
            {
                "income" or "收入" => TransactionType.Income,
                "expense" or "支出" => TransactionType.Expense,
                "transfer" or "转账" => TransactionType.Transfer,
                _ => (TransactionType?)null
            };

            if (!type.HasValue) continue;
            if (!decimal.TryParse(parts[2].Trim(), out var amount)) continue;
            if (!DateTime.TryParse(parts[0].Trim(), out var date)) continue;

            var note = parts.Length > 3 ? parts[3].Trim().Trim('"') : null;

            var transaction = new Transaction
            {
                Type = type.Value,
                Amount = amount,
                OccurredAt = date.ToUniversalTime(),
                AccountId = accountId,
                Note = note
            };

            await AddTransactionAsync(transaction);
            count++;
        }

        return count;
    }
}
