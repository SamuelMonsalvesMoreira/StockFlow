using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Data;
using StockFlow.Api.Models;

namespace StockFlow.Api.Repositories;

public sealed class EfInventoryRepository(StockFlowDbContext dbContext) : IInventoryRepository
{
    public async Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        bool lowStockOnly,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Supplier);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(product =>
                product.Name.Contains(term) || product.Sku.Contains(term));
        }

        if (lowStockOnly)
        {
            query = query.Where(product => product.QuantityInStock <= product.MinimumStock);
        }

        return await query
            .OrderBy(product => product.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Include(product => product.Supplier)
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<Product?> GetProductForUpdateAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Products
            .Include(product => product.Category)
            .Include(product => product.Supplier)
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(product => product.Sku == sku, cancellationToken);

    public async Task AddProductAsync(Product product, CancellationToken cancellationToken) =>
        await dbContext.Products.AddAsync(product, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken) =>
        await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

    public Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Categories.FirstOrDefaultAsync(category => category.Id == id, cancellationToken);

    public Task<bool> CategoryNameExistsAsync(
        string name,
        CancellationToken cancellationToken) =>
        dbContext.Categories.AnyAsync(category => category.Name == name, cancellationToken);

    public async Task AddCategoryAsync(Category category, CancellationToken cancellationToken) =>
        await dbContext.Categories.AddAsync(category, cancellationToken);

    public async Task<IReadOnlyList<Supplier>> GetSuppliersAsync(
        CancellationToken cancellationToken) =>
        await dbContext.Suppliers
            .AsNoTracking()
            .OrderBy(supplier => supplier.Name)
            .ToListAsync(cancellationToken);

    public Task<Supplier?> GetSupplierByIdAsync(int id, CancellationToken cancellationToken) =>
        dbContext.Suppliers.FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);

    public Task<bool> SupplierNameExistsAsync(
        string name,
        CancellationToken cancellationToken) =>
        dbContext.Suppliers.AnyAsync(supplier => supplier.Name == name, cancellationToken);

    public async Task AddSupplierAsync(Supplier supplier, CancellationToken cancellationToken) =>
        await dbContext.Suppliers.AddAsync(supplier, cancellationToken);

    public async Task AddMovementAsync(
        StockMovement movement,
        CancellationToken cancellationToken) =>
        await dbContext.StockMovements.AddAsync(movement, cancellationToken);

    public async Task<IReadOnlyList<StockMovement>> GetMovementsAsync(
        int productId,
        CancellationToken cancellationToken) =>
        await dbContext.StockMovements
            .AsNoTracking()
            .Where(movement => movement.ProductId == productId)
            .OrderByDescending(movement => movement.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StockMovement>> GetAllMovementsAsync(
        CancellationToken cancellationToken) =>
        await dbContext.StockMovements
            .AsNoTracking()
            .OrderByDescending(movement => movement.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
