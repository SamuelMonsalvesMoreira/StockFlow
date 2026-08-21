using StockFlow.Api.Models;

namespace StockFlow.Api.Repositories;

public interface IInventoryRepository
{
    Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        bool lowStockOnly,
        CancellationToken cancellationToken);

    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken);

    Task<Product?> GetProductForUpdateAsync(int id, CancellationToken cancellationToken);

    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken);

    Task AddProductAsync(Product product, CancellationToken cancellationToken);

    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> CategoryNameExistsAsync(string name, CancellationToken cancellationToken);

    Task AddCategoryAsync(Category category, CancellationToken cancellationToken);

    Task<IReadOnlyList<Supplier>> GetSuppliersAsync(CancellationToken cancellationToken);

    Task<Supplier?> GetSupplierByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> SupplierNameExistsAsync(string name, CancellationToken cancellationToken);

    Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken);

    Task AddMovementAsync(StockMovement movement, CancellationToken cancellationToken);

    Task<IReadOnlyList<StockMovement>> GetMovementsAsync(
        int productId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StockMovement>> GetAllMovementsAsync(
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
