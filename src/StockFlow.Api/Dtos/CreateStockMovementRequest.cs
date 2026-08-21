using System.ComponentModel.DataAnnotations;
using StockFlow.Api.Models;

namespace StockFlow.Api.Dtos;

public sealed class CreateStockMovementRequest
{
    [EnumDataType(typeof(StockMovementType))]
    public StockMovementType Type { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    [StringLength(250)]
    public string? Note { get; init; }
}
