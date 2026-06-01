using WealthOS.Domain.Common;
using WealthOS.Domain.Enums;

namespace WealthOS.Domain.Entities;

public class Account : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "CNY";
    public string Institution { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool IsActive { get; set; } = true;
}
