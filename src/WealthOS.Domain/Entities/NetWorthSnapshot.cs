using WealthOS.Domain.Common;

namespace WealthOS.Domain.Entities;

public class NetWorthSnapshot : BaseEntity
{
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public DateTime SnapshotDate { get; set; }
}
