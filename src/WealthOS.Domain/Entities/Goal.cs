using WealthOS.Domain.Common;
using WealthOS.Domain.Enums;

namespace WealthOS.Domain.Entities;

public class Goal : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.InProgress;
    public string? Icon { get; set; }
    public string? Note { get; set; }
}
