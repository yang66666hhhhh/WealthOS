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
        return assets.Select(a => new AssetCardDto
        {
            Id = a.Id,
            Name = a.Name,
            Type = a.Type,
            CurrentValue = a.CurrentValue,
            InitialValue = a.InitialValue,
            Currency = a.Currency,
            Institution = a.Institution,
            Note = a.Note,
            ChangeAmount = a.CurrentValue - a.InitialValue,
            ChangePercentage = a.InitialValue > 0 ? (a.CurrentValue - a.InitialValue) / a.InitialValue * 100 : 0
        });
    }

    public async Task<IEnumerable<AssetCardDto>> GetAssetsByTypeAsync(AssetType type)
    {
        var assets = await _repo.GetByTypeAsync(type);
        return assets.Select(a => new AssetCardDto
        {
            Id = a.Id,
            Name = a.Name,
            Type = a.Type,
            CurrentValue = a.CurrentValue,
            InitialValue = a.InitialValue,
            Currency = a.Currency,
            Institution = a.Institution,
            Note = a.Note,
            ChangeAmount = a.CurrentValue - a.InitialValue,
            ChangePercentage = a.InitialValue > 0 ? (a.CurrentValue - a.InitialValue) / a.InitialValue * 100 : 0
        });
    }

    public async Task<Guid> AddAssetAsync(Asset asset) => await _repo.AddAsync(asset);
    public async Task<bool> UpdateAssetAsync(Asset asset) => await _repo.UpdateAsync(asset);
    public async Task<bool> DeleteAssetAsync(Guid id) => await _repo.DeleteAsync(id);
    public async Task<Asset?> GetAssetAsync(Guid id) => await _repo.GetByIdAsync(id);
}
