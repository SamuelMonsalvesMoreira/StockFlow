using StockFlow.Api.Dtos;
using StockFlow.Api.Models;

namespace StockFlow.Api.Mappings;

public static class ResponseMappings
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.UnitPrice,
        product.QuantityInStock,
        product.MinimumStock,
        product.MaximumStock,
        product.SuggestedReorderQuantity,
        product.CategoryId,
        product.Category?.Name,
        product.SupplierId,
        product.Supplier?.Name,
        product.IsLowStock,
        product.StockValue,
        product.IsActive,
        product.CreatedAtUtc);

    public static StockMovementResponse ToResponse(this StockMovement movement) => new(
        movement.Id,
        movement.ProductId,
        movement.Type,
        movement.Quantity,
        movement.ResultingBalance,
        movement.Note,
        movement.PerformedByName,
        movement.PerformedByEmail,
        movement.CreatedAtUtc);

    public static CategoryResponse ToResponse(this Category category) => new(
        category.Id,
        category.Name,
        category.IsActive,
        category.CreatedAtUtc);

    public static SupplierResponse ToResponse(this Supplier supplier) => new(
        supplier.Id,
        supplier.Name,
        supplier.ContactName,
        supplier.Email,
        supplier.Phone,
        supplier.IsActive,
        supplier.CreatedAtUtc);
}
