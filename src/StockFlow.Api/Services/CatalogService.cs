using StockFlow.Api.Domain.Exceptions;
using StockFlow.Api.Dtos;
using StockFlow.Api.Mappings;
using StockFlow.Api.Models;
using StockFlow.Api.Repositories;

namespace StockFlow.Api.Services;

public sealed class CatalogService(IInventoryRepository repository) : ICatalogService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var categories = await repository.GetCategoriesAsync(cancellationToken);
        return categories.Select(category => category.ToResponse()).ToList();
    }

    public async Task<CategoryResponse> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await repository.CategoryNameExistsAsync(name, cancellationToken))
        {
            throw new ConflictException($"Já existe uma categoria chamada '{name}'.");
        }

        var category = new Category(name);
        await repository.AddCategoryAsync(category, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return category.ToResponse();
    }

    public async Task<IReadOnlyList<SupplierResponse>> GetSuppliersAsync(
        CancellationToken cancellationToken)
    {
        var suppliers = await repository.GetSuppliersAsync(cancellationToken);
        return suppliers.Select(supplier => supplier.ToResponse()).ToList();
    }

    public async Task<SupplierResponse> CreateSupplierAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (await repository.SupplierNameExistsAsync(name, cancellationToken))
        {
            throw new ConflictException($"Já existe um fornecedor chamado '{name}'.");
        }

        var supplier = new Supplier(
            name,
            request.ContactName,
            request.Email,
            request.Phone);

        await repository.AddSupplierAsync(supplier, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return supplier.ToResponse();
    }
}
