namespace StockFlow.Api.Dtos;

public sealed record InventorySummaryResponse(
    int TotalProducts,
    int LowStockProducts,
    int TotalUnits,
    decimal TotalStockValue);
