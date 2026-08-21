namespace StockFlow.Api.Dtos;

public sealed record ProductResponse(
    int Id,
    string Sku,
    string Name,
    decimal UnitPrice,
    int QuantityInStock,
    int MinimumStock,
    int MaximumStock,
    int SuggestedReorderQuantity,
    int? CategoryId,
    string? CategoryName,
    int? SupplierId,
    string? SupplierName,
    bool IsLowStock,
    decimal StockValue,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
