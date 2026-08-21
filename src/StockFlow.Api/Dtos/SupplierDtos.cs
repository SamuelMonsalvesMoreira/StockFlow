using System.ComponentModel.DataAnnotations;

namespace StockFlow.Api.Dtos;

public sealed class CreateSupplierRequest
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(120)]
    public string? ContactName { get; init; }

    [EmailAddress]
    [StringLength(180)]
    public string? Email { get; init; }

    [StringLength(30)]
    public string? Phone { get; init; }
}

public sealed record SupplierResponse(
    int Id,
    string Name,
    string? ContactName,
    string? Email,
    string? Phone,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
