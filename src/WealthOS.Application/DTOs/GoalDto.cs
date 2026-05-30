using WealthOS.Domain.Enums;

namespace WealthOS.Application.DTOs;

public record GoalDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public decimal CurrentAmount { get; init; }
    public DateTime TargetDate { get; init; }
    public GoalStatus Status { get; init; }
    public string? Icon { get; init; }
    public decimal Progress => TargetAmount > 0 ? CurrentAmount / TargetAmount * 100 : 0;
    public int EstimatedMonthsRemaining
    {
        get
        {
            if (CurrentAmount >= TargetAmount) return 0;
            var remaining = TargetAmount - CurrentAmount;
            var now = DateTime.UtcNow;
            if (TargetDate <= now) return 0;
            var monthsLeft = (TargetDate.Year - now.Year) * 12 + TargetDate.Month - now.Month;
            return Math.Max(monthsLeft, 0);
        }
    }
}
