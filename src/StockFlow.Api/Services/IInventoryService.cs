using StockFlow.Api.Dtos;

namespace StockFlow.Api.Services;

public interface IInventoryService
{
    Task<IReadOnlyList<ProductResponse>> GetProductsAsync(
        string? search,
        bool lowStockOnly,
        CancellationToken cancellationToken);

    Task<ProductResponse> GetProductByIdAsync(int id, CancellationToken cancellationToken);

    Task<ProductResponse> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken);

    Task<ProductResponse> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken);

    Task<StockMovementResponse> RegisterMovementAsync(
        int productId,
        CreateStockMovementRequest request,
        string performedByName,
        string performedByEmail,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockMovementResponse>> GetMovementsAsync(
        int productId,
        CancellationToken cancellationToken);

    Task<InventorySummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
}
