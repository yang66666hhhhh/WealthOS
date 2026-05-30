using WealthOS.Domain.Common;
using WealthOS.Domain.Enums;

namespace WealthOS.Domain.Entities;

public class Asset : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal InitialValue { get; set; }
    public string Currency { get; set; } = "CNY";
    public string Institution { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal? DepreciationRate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public bool IsActive { get; set; } = true;
}
