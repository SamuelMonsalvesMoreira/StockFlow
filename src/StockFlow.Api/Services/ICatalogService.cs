using StockFlow.Api.Dtos;

namespace StockFlow.Api.Services;

public interface ICatalogService
{
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken);

    Task<CategoryResponse> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SupplierResponse>> GetSuppliersAsync(
        CancellationToken cancellationToken);

    Task<SupplierResponse> CreateSupplierAsync(
        CreateSupplierRequest request,
        CancellationToken cancellationToken);
}
