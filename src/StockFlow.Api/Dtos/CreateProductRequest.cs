using System.ComponentModel.DataAnnotations;

namespace StockFlow.Api.Dtos;

public sealed class CreateProductRequest
{
    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string Sku { get; init; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Range(
        typeof(decimal),
        "0.01",
        "999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal UnitPrice { get; init; }

    [Range(0, int.MaxValue)]
    public int MinimumStock { get; init; }

    [Range(0, int.MaxValue)]
    public int MaximumStock { get; init; }

    [Range(1, int.MaxValue)]
    public int? CategoryId { get; init; }

    [Range(1, int.MaxValue)]
    public int? SupplierId { get; init; }
}
