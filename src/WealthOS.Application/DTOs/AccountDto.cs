using WealthOS.Application.Interfaces;
using WealthOS.Domain.Enums;

namespace WealthOS.Application.DTOs;

public record AccountDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public AccountType Type { get; init; }
    public decimal Balance { get; init; }
    public string Currency { get; init; } = "CNY";
    public string Institution { get; init; } = string.Empty;
    public string? Note { get; init; }
}
