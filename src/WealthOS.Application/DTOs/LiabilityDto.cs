using WealthOS.Domain.Enums;

namespace WealthOS.Application.DTOs;

public record LiabilityDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public LiabilityType Type { get; init; }
    public decimal Balance { get; init; }
    public decimal InterestRate { get; init; }
    public decimal MonthlyPayment { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string Currency { get; init; } = "CNY";
    public string? Institution { get; init; }
}
