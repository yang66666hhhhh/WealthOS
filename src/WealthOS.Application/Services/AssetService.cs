using WealthOS.Application.DTOs;
using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.Services;

public class AssetService
{
    private readonly IAssetRepository _repo;

    public AssetService(IAssetRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<AssetCardDto>> GetAllAssetsAsync()
    {
        var assets = await _repo.GetAllAsync();
        return assets.Select(ToDto);
    }

    public async Task<IEnumerable<AssetCardDto>> GetAssetsByTypeAsync(AssetType type)
    {
        var assets = await _repo.GetByTypeAsync(type);
        return assets.Select(ToDto);
    }

    public async Task<Guid> AddAssetAsync(Asset asset) => await _repo.AddAsync(asset);
    public async Task<bool> UpdateAssetAsync(Asset asset) => await _repo.UpdateAsync(asset);
    public async Task<bool> DeleteAssetAsync(Guid id) => await _repo.DeleteAsync(id);
    public async Task<Asset?> GetAssetAsync(Guid id) => await _repo.GetByIdAsync(id);

    private static AssetCardDto ToDto(Asset a)
    {
        int? ageMonths = null;
        decimal? depreciatedValue = null;

        if (a.PurchaseDate.HasValue && a.DepreciationRate.HasValue && a.DepreciationRate.Value > 0)
        {
            var now = DateTime.UtcNow;
            ageMonths = (now.Year - a.PurchaseDate.Value.Year) * 12 + now.Month - a.PurchaseDate.Value.Month;
            var depreciationFactor = 1m - (a.DepreciationRate.Value / 100m * ageMonths.Value / 12m);
            depreciatedValue = Math.Max(a.InitialValue * depreciationFactor, 0);
        }

        return new AssetCardDto
        {
            Id = a.Id,
            Name = a.Name,
            Type = a.Type,
            CurrentValue = a.CurrentValue,
            InitialValue = a.InitialValue,
            Currency = a.Currency,
            Institution = a.Institution,
            Note = a.Note,
            DepreciationRate = a.DepreciationRate,
            PurchaseDate = a.PurchaseDate,
            ChangeAmount = a.CurrentValue - a.InitialValue,
            ChangePercentage = a.InitialValue > 0 ? (a.CurrentValue - a.InitialValue) / a.InitialValue * 100 : 0,
            AgeMonths = ageMonths,
            DepreciatedValue = depreciatedValue
        };
    }
}
