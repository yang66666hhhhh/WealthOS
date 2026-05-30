using WealthOS.Domain.Enums;

namespace WealthOS.Application.DTOs;

public record AssetCardDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public AssetType Type { get; init; }
    public decimal CurrentValue { get; init; }
    public decimal InitialValue { get; init; }
    public string Currency { get; init; } = "CNY";
    public string Institution { get; init; } = string.Empty;
    public string? Note { get; init; }
    public decimal ChangeAmount { get; init; }
    public decimal ChangePercentage { get; init; }
}
