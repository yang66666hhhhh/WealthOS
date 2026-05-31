using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class InvestmentService
{
    private readonly IInvestmentHoldingRepository _repo;

    public InvestmentService(IInvestmentHoldingRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<InvestmentHoldingDto>> GetAllHoldingsAsync()
    {
        var holdings = await _repo.GetAllAsync();
        return holdings.Select(ToDto);
    }

    public async Task<IEnumerable<InvestmentHoldingDto>> GetByAssetTypeAsync(AssetType type)
    {
        var holdings = await _repo.GetByAssetTypeAsync(type);
        return holdings.Select(ToDto);
    }

    public async Task<Guid> AddHoldingAsync(InvestmentHolding holding) => await _repo.AddAsync(holding);
    public async Task<bool> UpdateHoldingAsync(InvestmentHolding holding) => await _repo.UpdateAsync(holding);
    public async Task<bool> DeleteHoldingAsync(Guid id) => await _repo.DeleteAsync(id);
    public async Task<InvestmentHolding?> GetHoldingAsync(Guid id) => await _repo.GetByIdAsync(id);

    private static InvestmentHoldingDto ToDto(InvestmentHolding h) => new()
    {
        Id = h.Id,
        Symbol = h.Symbol,
        Name = h.Name,
        AssetType = h.AssetType,
        Quantity = h.Quantity,
        AverageCost = h.AverageCost,
        CurrentPrice = h.CurrentPrice,
        Currency = h.Currency,
        Note = h.Note
    };
}
