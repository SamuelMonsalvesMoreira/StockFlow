using StockFlow.Api.Models;

namespace StockFlow.Api.Dtos;

public sealed record ReportOverviewResponse(
    DateTimeOffset GeneratedAtUtc,
    InventorySummaryResponse Summary,
    IReadOnlyList<CategoryReportItem> Categories,
    IReadOnlyList<LowStockReportItem> LowStockProducts,
    IReadOnlyList<MovementReportItem> RecentMovements);

public sealed record CategoryReportItem(
    string CategoryName,
    int ProductCount,
    int TotalUnits,
    decimal TotalStockValue);

public sealed record LowStockReportItem(
    int ProductId,
    string Sku,
    string ProductName,
    int QuantityInStock,
    int MinimumStock,
    int MaximumStock,
    int SuggestedReorderQuantity);

public sealed record MovementReportItem(
    int Id,
    int ProductId,
    string Sku,
    string ProductName,
    StockMovementType Type,
    int Quantity,
    int ResultingBalance,
    string PerformedByName,
    DateTimeOffset CreatedAtUtc);
