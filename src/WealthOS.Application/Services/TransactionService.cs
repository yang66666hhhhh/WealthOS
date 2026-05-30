using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class TransactionService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IAccountRepository _accountRepo;
    private readonly ICategoryRepository _categoryRepo;

    public TransactionService(
        ITransactionRepository transactionRepo,
        IAccountRepository accountRepo,
        ICategoryRepository categoryRepo)
    {
        _transactionRepo = transactionRepo;
        _accountRepo = accountRepo;
        _categoryRepo = categoryRepo;
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
                CategoryName = t.CategoryId.HasValue && categories.TryGetValue(t.CategoryId.Value, out var cat) ? cat.Name : null,
                AccountName = accounts.TryGetValue(t.AccountId, out var acc) ? acc.Name : "",
                Note = t.Note,
                OccurredAt = t.OccurredAt
            });
    }

    public async Task<Guid> AddTransactionAsync(Transaction transaction)
    {
        var id = await _transactionRepo.AddAsync(transaction);

        var account = await _accountRepo.GetByIdAsync(transaction.AccountId);
        if (account != null)
        {
            account.Balance += transaction.Type switch
            {
                TransactionType.Income => transaction.Amount,
                TransactionType.Expense => -transaction.Amount,
                TransactionType.Transfer => -transaction.Amount,
                _ => 0
            };
            account.UpdatedAt = DateTime.UtcNow;
            await _accountRepo.UpdateAsync(account);
        }

        if (transaction.Type == TransactionType.Transfer && transaction.ToAccountId.HasValue)
        {
            var toAccount = await _accountRepo.GetByIdAsync(transaction.ToAccountId.Value);
            if (toAccount != null)
            {
                toAccount.Balance += transaction.Amount;
                toAccount.UpdatedAt = DateTime.UtcNow;
                await _accountRepo.UpdateAsync(toAccount);
            }
        }

        return id;
    }

    public async Task<bool> DeleteTransactionAsync(Guid id) => await _transactionRepo.DeleteAsync(id);
}
