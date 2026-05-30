using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class AccountService
{
    private readonly IAccountRepository _repo;

    public AccountService(IAccountRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await _repo.GetAllAsync();
        return accounts.Select(a => new AccountDto
        {
            Id = a.Id,
            Name = a.Name,
            Type = a.Type,
            Balance = a.Balance,
            Currency = a.Currency,
            Institution = a.Institution,
            Note = a.Note
        });
    }

    public async Task<Guid> AddAccountAsync(Account account) => await _repo.AddAsync(account);
    public async Task<bool> UpdateAccountAsync(Account account) => await _repo.UpdateAsync(account);
    public async Task<bool> DeleteAccountAsync(Guid id) => await _repo.DeleteAsync(id);
    public async Task<Account?> GetAccountAsync(Guid id) => await _repo.GetByIdAsync(id);
}
