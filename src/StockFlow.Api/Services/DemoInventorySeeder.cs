using StockFlow.Api.Dtos;
using StockFlow.Api.Models;

namespace StockFlow.Api.Services;

public sealed class DemoInventorySeeder(
    IInventoryService inventoryService,
    ICatalogService catalogService)
{
    private const string DemoUserName = "Sistema demonstrativo";
    private const string DemoUserEmail = "demo@stockflow.local";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingProducts = await inventoryService.GetProductsAsync(
            search: null,
            lowStockOnly: false,
            cancellationToken);

        if (existingProducts.Count > 0)
        {
            return;
        }

        var informatica = await catalogService.CreateCategoryAsync(
            new CreateCategoryRequest { Name = "Informática" },
            cancellationToken);
        var perifericos = await catalogService.CreateCategoryAsync(
            new CreateCategoryRequest { Name = "Periféricos" },
            cancellationToken);
        var escritorio = await catalogService.CreateCategoryAsync(
            new CreateCategoryRequest { Name = "Escritório" },
            cancellationToken);

        var techSupplier = await catalogService.CreateSupplierAsync(
            new CreateSupplierRequest
            {
                Name = "Tech Distribuidora Demo",
                ContactName = "Equipe comercial",
                Email = "comercial@tech-demo.local"
            },
            cancellationToken);
        var officeSupplier = await catalogService.CreateSupplierAsync(
            new CreateSupplierRequest
            {
                Name = "Office Brasil Demo",
                ContactName = "Atendimento",
                Email = "contato@office-demo.local"
            },
            cancellationToken);

        await CreateProductWithInitialStockAsync(
            "NOTE-DELL-01",
            "Notebook Dell Inspiron",
            4500m,
            minimumStock: 3,
            maximumStock: 15,
            initialStock: 8,
            informatica.Id,
            techSupplier.Id,
            cancellationToken);
        await CreateProductWithInitialStockAsync(
            "MOUSE-LOGI-01",
            "Mouse Logitech M185",
            89.90m,
            minimumStock: 10,
            maximumStock: 40,
            initialStock: 7,
            perifericos.Id,
            techSupplier.Id,
            cancellationToken);
        await CreateProductWithInitialStockAsync(
            "MON-LG-24",
            "Monitor LG 24 polegadas",
            899m,
            minimumStock: 4,
            maximumStock: 18,
            initialStock: 12,
            informatica.Id,
            techSupplier.Id,
            cancellationToken);
        await CreateProductWithInitialStockAsync(
            "CAD-ERG-01",
            "Cadeira ergonômica",
            1190m,
            minimumStock: 2,
            maximumStock: 10,
            initialStock: 3,
            escritorio.Id,
            officeSupplier.Id,
            cancellationToken);
        await CreateProductWithInitialStockAsync(
            "TECL-RED-01",
            "Teclado mecânico Red Switch",
            349m,
            minimumStock: 5,
            maximumStock: 20,
            initialStock: 4,
            perifericos.Id,
            techSupplier.Id,
            cancellationToken);
    }

    private async Task CreateProductWithInitialStockAsync(
        string sku,
        string name,
        decimal unitPrice,
        int minimumStock,
        int maximumStock,
        int initialStock,
        int categoryId,
        int supplierId,
        CancellationToken cancellationToken)
    {
        var product = await inventoryService.CreateProductAsync(
            new CreateProductRequest
            {
                Sku = sku,
                Name = name,
                UnitPrice = unitPrice,
                MinimumStock = minimumStock,
                MaximumStock = maximumStock,
                CategoryId = categoryId,
                SupplierId = supplierId
            },
            cancellationToken);

        await inventoryService.RegisterMovementAsync(
            product.Id,
            new CreateStockMovementRequest
            {
                Type = StockMovementType.Entry,
                Quantity = initialStock,
                Note = "Carga inicial do ambiente demonstrativo"
            },
            DemoUserName,
            DemoUserEmail,
            cancellationToken);
    }
}
