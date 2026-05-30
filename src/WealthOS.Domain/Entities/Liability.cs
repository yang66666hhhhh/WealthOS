using WealthOS.Domain.Common;
using WealthOS.Domain.Enums;

namespace WealthOS.Domain.Entities;

public class Liability : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public LiabilityType Type { get; set; }
    public decimal Balance { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Currency { get; set; } = "CNY";
    public string? Institution { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
}
