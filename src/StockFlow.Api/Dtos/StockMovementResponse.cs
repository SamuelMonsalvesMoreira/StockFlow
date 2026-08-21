using StockFlow.Api.Models;

namespace StockFlow.Api.Dtos;

public sealed record StockMovementResponse(
    int Id,
    int ProductId,
    StockMovementType Type,
    int Quantity,
    int ResultingBalance,
    string? Note,
    string PerformedByName,
    string PerformedByEmail,
    DateTimeOffset CreatedAtUtc);
