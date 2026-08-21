using StockFlow.Api.Models;

namespace StockFlow.Api.Repositories;

public sealed class MemoryInventoryRepository : IInventoryRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<int, Product> _products = [];
    private readonly Dictionary<int, Category> _categories = [];
    private readonly Dictionary<int, Supplier> _suppliers = [];
    private readonly List<StockMovement> _movements = [];
    private int _nextProductId;
    private int _nextCategoryId;
    private int _nextSupplierId;
    private int _nextMovementId;

    public Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        bool lowStockOnly,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            IEnumerable<Product> query = _products.Values;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(product =>
                    product.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || product.Sku.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            if (lowStockOnly)
            {
                query = query.Where(product => product.IsLowStock);
            }

            return Task.FromResult<IReadOnlyList<Product>>(
                query.OrderBy(product => product.Name).ToList());
        }
    }

    public Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }
    }

    public Task<Product?> GetProductForUpdateAsync(int id, CancellationToken cancellationToken) =>
        GetProductByIdAsync(id, cancellationToken);

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_products.Values.Any(product =>
                product.Sku.Equals(sku, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public Task AddProductAsync(Product product, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            product.AssignId(++_nextProductId);
            _products.Add(product.Id, product);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<Category>>(
                _categories.Values.OrderBy(category => category.Name).ToList());
        }
    }

    public Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _categories.TryGetValue(id, out var category);
            return Task.FromResult(category);
        }
    }

    public Task<bool> CategoryNameExistsAsync(
        string name,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_categories.Values.Any(category =>
                category.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public Task AddCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            category.AssignId(++_nextCategoryId);
            _categories.Add(category.Id, category);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Supplier>> GetSuppliersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<Supplier>>(
                _suppliers.Values.OrderBy(supplier => supplier.Name).ToList());
        }
    }

    public Task<Supplier?> GetSupplierByIdAsync(int id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            _suppliers.TryGetValue(id, out var supplier);
            return Task.FromResult(supplier);
        }
    }

    public Task<bool> SupplierNameExistsAsync(
        string name,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(_suppliers.Values.Any(supplier =>
                supplier.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            supplier.AssignId(++_nextSupplierId);
            _suppliers.Add(supplier.Id, supplier);
        }

        return Task.CompletedTask;
    }

    public Task AddMovementAsync(StockMovement movement, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            movement.AssignId(++_nextMovementId);
            _movements.Add(movement);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StockMovement>> GetMovementsAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<StockMovement>>(
                _movements
                    .Where(movement => movement.ProductId == productId)
                    .OrderByDescending(movement => movement.CreatedAtUtc)
                    .ToList());
        }
    }

    public Task<IReadOnlyList<StockMovement>> GetAllMovementsAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<StockMovement>>(
                _movements.OrderByDescending(movement => movement.CreatedAtUtc).ToList());
        }
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
