using StockFlow.Api.Domain.Exceptions;
using StockFlow.Api.Dtos;
using StockFlow.Api.Mappings;
using StockFlow.Api.Models;
using StockFlow.Api.Repositories;

namespace StockFlow.Api.Services;

public sealed class InventoryService(IInventoryRepository repository) : IInventoryService
{
    public async Task<IReadOnlyList<ProductResponse>> GetProductsAsync(
        string? search,
        bool lowStockOnly,
        CancellationToken cancellationToken)
    {
        var products = await repository.GetProductsAsync(
            search,
            lowStockOnly,
            cancellationToken);

        return products.Select(product => product.ToResponse()).ToList();
    }

    public async Task<ProductResponse> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetProductByIdAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"O produto {id} não foi encontrado.");

        return product.ToResponse();
    }

    public async Task<ProductResponse> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedSku = request.Sku.Trim().ToUpperInvariant();

        if (await repository.SkuExistsAsync(normalizedSku, cancellationToken))
        {
            throw new ConflictException($"Já existe um produto com o SKU '{normalizedSku}'.");
        }

        var (category, supplier) = await GetCatalogReferencesAsync(
            request.CategoryId,
            request.SupplierId,
            cancellationToken);

        var product = new Product(
            normalizedSku,
            request.Name,
            request.UnitPrice,
            request.MinimumStock,
            request.MaximumStock,
            category,
            supplier);

        await repository.AddProductAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return product.ToResponse();
    }

    public async Task<ProductResponse> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetProductForUpdateAsync(id, cancellationToken)
            ?? throw new ResourceNotFoundException($"O produto {id} não foi encontrado.");

        var (category, supplier) = await GetCatalogReferencesAsync(
            request.CategoryId,
            request.SupplierId,
            cancellationToken);

        product.UpdateDetails(
            request.Name,
            request.UnitPrice,
            request.MinimumStock,
            request.MaximumStock,
            category,
            supplier);

        await repository.SaveChangesAsync(cancellationToken);
        return product.ToResponse();
    }

    public async Task<StockMovementResponse> RegisterMovementAsync(
        int productId,
        CreateStockMovementRequest request,
        string performedByName,
        string performedByEmail,
        CancellationToken cancellationToken)
    {
        var product = await repository.GetProductForUpdateAsync(productId, cancellationToken)
            ?? throw new ResourceNotFoundException($"O produto {productId} não foi encontrado.");

        var movement = product.RegisterMovement(
            request.Type,
            request.Quantity,
            request.Note,
            performedByName,
            performedByEmail);

        await repository.AddMovementAsync(movement, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return movement.ToResponse();
    }

    public async Task<IReadOnlyList<StockMovementResponse>> GetMovementsAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        _ = await repository.GetProductByIdAsync(productId, cancellationToken)
            ?? throw new ResourceNotFoundException($"O produto {productId} não foi encontrado.");

        var movements = await repository.GetMovementsAsync(productId, cancellationToken);
        return movements.Select(movement => movement.ToResponse()).ToList();
    }

    public async Task<InventorySummaryResponse> GetSummaryAsync(
        CancellationToken cancellationToken)
    {
        var products = await repository.GetProductsAsync(
            search: null,
            lowStockOnly: false,
            cancellationToken);

        return new InventorySummaryResponse(
            products.Count,
            products.Count(product => product.IsLowStock),
            products.Sum(product => product.QuantityInStock),
            products.Sum(product => product.StockValue));
    }

    private async Task<(Category? Category, Supplier? Supplier)> GetCatalogReferencesAsync(
        int? categoryId,
        int? supplierId,
        CancellationToken cancellationToken)
    {
        Category? category = null;
        Supplier? supplier = null;

        if (categoryId.HasValue)
        {
            category = await repository.GetCategoryByIdAsync(
                categoryId.Value,
                cancellationToken)
                ?? throw new ResourceNotFoundException(
                    $"A categoria {categoryId.Value} não foi encontrada.");
        }

        if (supplierId.HasValue)
        {
            supplier = await repository.GetSupplierByIdAsync(
                supplierId.Value,
                cancellationToken)
                ?? throw new ResourceNotFoundException(
                    $"O fornecedor {supplierId.Value} não foi encontrado.");
        }

        return (category, supplier);
    }
}
