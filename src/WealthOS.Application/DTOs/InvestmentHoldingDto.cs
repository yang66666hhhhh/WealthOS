using WealthOS.Domain.Enums;

namespace WealthOS.Application.DTOs;

public record InvestmentHoldingDto
{
    public Guid Id { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public AssetType AssetType { get; init; }
    public decimal Quantity { get; init; }
    public decimal AverageCost { get; init; }
    public decimal CurrentPrice { get; init; }
    public string Currency { get; init; } = "CNY";
    public string? Note { get; init; }
    public decimal TotalCost => Quantity * AverageCost;
    public decimal TotalValue => Quantity * CurrentPrice;
    public decimal ProfitLoss => TotalValue - TotalCost;
    public decimal ProfitLossPercentage => TotalCost > 0 ? ProfitLoss / TotalCost * 100 : 0;
}
