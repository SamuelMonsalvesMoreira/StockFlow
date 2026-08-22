using StockFlow.Api.Repositories;
using StockFlow.Api.Services;

namespace StockFlow.Api.Tests;

public sealed class DemoInventorySeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesUsefulSampleDataAndIsIdempotent()
    {
        var repository = new MemoryInventoryRepository();
        var inventoryService = new InventoryService(repository);
        var catalogService = new CatalogService(repository);
        var seeder = new DemoInventorySeeder(inventoryService, catalogService);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var products = await inventoryService.GetProductsAsync(null, false, CancellationToken.None);
        var categories = await catalogService.GetCategoriesAsync(CancellationToken.None);
        var suppliers = await catalogService.GetSuppliersAsync(CancellationToken.None);
        var summary = await inventoryService.GetSummaryAsync(CancellationToken.None);

        Assert.Equal(5, products.Count);
        Assert.Equal(3, categories.Count);
        Assert.Equal(2, suppliers.Count);
        Assert.Equal(34, summary.TotalUnits);
        Assert.Equal(2, summary.LowStockProducts);
        Assert.All(products, product => Assert.NotEqual(0, product.QuantityInStock));
    }
}
