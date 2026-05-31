using WealthOS.Application.Interfaces;
using WealthOS.Domain.Entities;

namespace WealthOS.Application.Services;

public class NetWorthService
{
    private readonly IAssetRepository _assetRepo;
    private readonly ILiabilityRepository _liabilityRepo;
    private readonly INetWorthRepository _netWorthRepo;

    public NetWorthService(
        IAssetRepository assetRepo,
        ILiabilityRepository liabilityRepo,
        INetWorthRepository netWorthRepo)
    {
        _assetRepo = assetRepo;
        _liabilityRepo = liabilityRepo;
        _netWorthRepo = netWorthRepo;
    }

    public async Task SaveSnapshotAsync()
    {
        var totalAssets = await _assetRepo.GetTotalValueAsync();
        var totalLiabilities = await _liabilityRepo.GetTotalBalanceAsync();

        var snapshot = new NetWorthSnapshot
        {
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            NetWorth = totalAssets - totalLiabilities,
            SnapshotDate = DateTime.UtcNow
        };

        await _netWorthRepo.AddAsync(snapshot);
    }

    public async Task SaveSnapshotIfNotExistsTodayAsync()
    {
        var latest = await _netWorthRepo.GetLatestAsync();
        if (latest != null && latest.SnapshotDate.Date == DateTime.UtcNow.Date)
            return;

        await SaveSnapshotAsync();
    }
}
