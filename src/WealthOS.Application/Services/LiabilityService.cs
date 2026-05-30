using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;

namespace WealthOS.Application.Services;

public class LiabilityService
{
    private readonly ILiabilityRepository _repo;

    public LiabilityService(ILiabilityRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<LiabilityDto>> GetAllLiabilitiesAsync()
    {
        var liabilities = await _repo.GetAllAsync();
        return liabilities.Select(l => new LiabilityDto
        {
            Id = l.Id,
            Name = l.Name,
            Type = l.Type,
            Balance = l.Balance,
            InterestRate = l.InterestRate,
            MonthlyPayment = l.MonthlyPayment,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Currency = l.Currency,
            Institution = l.Institution
        });
    }

    public async Task<Guid> AddLiabilityAsync(Liability liability) => await _repo.AddAsync(liability);
    public async Task<bool> UpdateLiabilityAsync(Liability liability) => await _repo.UpdateAsync(liability);
    public async Task<bool> DeleteLiabilityAsync(Guid id) => await _repo.DeleteAsync(id);
    public async Task<Liability?> GetLiabilityAsync(Guid id) => await _repo.GetByIdAsync(id);
}
