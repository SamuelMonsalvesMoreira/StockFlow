using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockFlow.Api.Domain.Exceptions;
using StockFlow.Api.Dtos;
using StockFlow.Api.Models;
using StockFlow.Api.Repositories;
using StockFlow.Api.Services;

namespace StockFlow.Api.Tests;

public sealed class InventoryServiceTests
{
    [Fact]
    public void CreateProductRequest_AcceptsValidDecimalInPortugueseCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
            var request = NewProduct(unitPrice: 4500m);
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                validationResults,
                validateAllProperties: true);

            Assert.True(isValid, string.Join("; ", validationResults.Select(result => result.ErrorMessage)));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public async Task CreateProductAsync_NormalizesSkuAndStartsWithZeroStock()
    {
        var service = CreateService();

        var product = await service.CreateProductAsync(
            NewProduct(sku: " note-01 "),
            CancellationToken.None);

        Assert.Equal("NOTE-01", product.Sku);
        Assert.Equal(0, product.QuantityInStock);
        Assert.True(product.IsLowStock);
        Assert.Equal(12, product.SuggestedReorderQuantity);
    }

    [Fact]
    public async Task CreateProductAsync_RejectsDuplicatedSkuIgnoringCase()
    {
        var service = CreateService();
        await service.CreateProductAsync(
            NewProduct(sku: "mouse-01"),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateProductAsync(
                NewProduct(sku: "MOUSE-01"),
                CancellationToken.None));

        Assert.Contains("MOUSE-01", exception.Message);
    }

    [Fact]
    public async Task RegisterMovementAsync_UpdatesBalanceForEntriesAndExits()
    {
        var service = CreateService();
        var product = await service.CreateProductAsync(
            NewProduct(),
            CancellationToken.None);

        var entry = await service.RegisterMovementAsync(
            product.Id,
            NewMovement(StockMovementType.Entry, 10),
            "Gestor de Teste",
            "gestor@teste.local",
            CancellationToken.None);
        var exit = await service.RegisterMovementAsync(
            product.Id,
            NewMovement(StockMovementType.Exit, 4),
            "Gestor de Teste",
            "gestor@teste.local",
            CancellationToken.None);

        Assert.Equal(10, entry.ResultingBalance);
        Assert.Equal(6, exit.ResultingBalance);
        Assert.Equal("Gestor de Teste", entry.PerformedByName);

        var updatedProduct = await service.GetProductByIdAsync(
            product.Id,
            CancellationToken.None);
        Assert.Equal(6, updatedProduct.QuantityInStock);
    }

    [Fact]
    public async Task RegisterMovementAsync_RejectsExitGreaterThanAvailableStock()
    {
        var service = CreateService();
        var product = await service.CreateProductAsync(
            NewProduct(),
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.RegisterMovementAsync(
                product.Id,
                NewMovement(StockMovementType.Exit, 1),
                "Gestor de Teste",
                "gestor@teste.local",
                CancellationToken.None));

        Assert.Contains("Saldo insuficiente", exception.Message);
    }

    [Fact]
    public async Task GetSummaryAsync_CalculatesLowStockUnitsAndValue()
    {
        var service = CreateService();
        var stockedProduct = await service.CreateProductAsync(
            NewProduct(sku: "PROD-01", unitPrice: 25m, minimumStock: 5),
            CancellationToken.None);
        await service.CreateProductAsync(
            NewProduct(sku: "PROD-02", unitPrice: 10m, minimumStock: 2),
            CancellationToken.None);
        await service.RegisterMovementAsync(
            stockedProduct.Id,
            NewMovement(StockMovementType.Entry, 10),
            "Gestor de Teste",
            "gestor@teste.local",
            CancellationToken.None);

        var summary = await service.GetSummaryAsync(CancellationToken.None);

        Assert.Equal(2, summary.TotalProducts);
        Assert.Equal(1, summary.LowStockProducts);
        Assert.Equal(10, summary.TotalUnits);
        Assert.Equal(250m, summary.TotalStockValue);
    }

    [Fact]
    public async Task UpdateProductAsync_ChangesDetailsAndPreservesSkuAndBalance()
    {
        var repository = new MemoryInventoryRepository();
        var inventoryService = new InventoryService(repository);
        var catalogService = new CatalogService(repository);
        var category = await catalogService.CreateCategoryAsync(
            new CreateCategoryRequest { Name = "Informática" },
            CancellationToken.None);
        var supplier = await catalogService.CreateSupplierAsync(
            new CreateSupplierRequest { Name = "Fornecedor Tech" },
            CancellationToken.None);
        var product = await inventoryService.CreateProductAsync(
            NewProduct(),
            CancellationToken.None);
        await inventoryService.RegisterMovementAsync(
            product.Id,
            NewMovement(StockMovementType.Entry, 5),
            "Gestor de Teste",
            "gestor@teste.local",
            CancellationToken.None);

        var updated = await inventoryService.UpdateProductAsync(
            product.Id,
            new UpdateProductRequest
            {
                Name = "Teclado atualizado",
                UnitPrice = 150m,
                MinimumStock = 6,
                MaximumStock = 20,
                CategoryId = category.Id,
                SupplierId = supplier.Id
            },
            CancellationToken.None);

        Assert.Equal("KEYBOARD-01", updated.Sku);
        Assert.Equal("Teclado atualizado", updated.Name);
        Assert.Equal(5, updated.QuantityInStock);
        Assert.Equal(15, updated.SuggestedReorderQuantity);
        Assert.Equal("Informática", updated.CategoryName);
        Assert.Equal("Fornecedor Tech", updated.SupplierName);
    }

    [Fact]
    public async Task CatalogService_RejectsDuplicatedCategoryIgnoringCase()
    {
        var service = new CatalogService(new MemoryInventoryRepository());
        await service.CreateCategoryAsync(
            new CreateCategoryRequest { Name = "Periféricos" },
            CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateCategoryAsync(
                new CreateCategoryRequest { Name = "periféricos" },
                CancellationToken.None));
    }

    [Fact]
    public async Task ReportService_BuildsCategoryLowStockAndAuditSections()
    {
        var repository = new MemoryInventoryRepository();
        var inventoryService = new InventoryService(repository);
        var catalogService = new CatalogService(repository);
        var reportService = new ReportService(repository);
        var category = await catalogService.CreateCategoryAsync(
            new CreateCategoryRequest { Name = "Informática" },
            CancellationToken.None);
        var product = await inventoryService.CreateProductAsync(
            NewProduct(
                sku: "NOTE-01",
                unitPrice: 50m,
                minimumStock: 6,
                maximumStock: 10,
                categoryId: category.Id),
            CancellationToken.None);
        await inventoryService.RegisterMovementAsync(
            product.Id,
            NewMovement(StockMovementType.Entry, 5),
            "Gestor de Teste",
            "gestor@teste.local",
            CancellationToken.None);

        var report = await reportService.GetOverviewAsync(CancellationToken.None);

        var categoryReport = Assert.Single(report.Categories);
        Assert.Equal("Informática", categoryReport.CategoryName);
        Assert.Equal(250m, categoryReport.TotalStockValue);
        Assert.Equal(5, Assert.Single(report.LowStockProducts).SuggestedReorderQuantity);
        Assert.Equal("Gestor de Teste", Assert.Single(report.RecentMovements).PerformedByName);
    }

    [Fact]
    public async Task ReportService_ExportsExcelFriendlyEscapedCsv()
    {
        var repository = new MemoryInventoryRepository();
        var inventoryService = new InventoryService(repository);
        var reportService = new ReportService(repository);
        await inventoryService.CreateProductAsync(
            NewProduct(sku: "CABO-01", name: "Cabo; USB"),
            CancellationToken.None);

        var csv = await reportService.ExportInventoryCsvAsync(CancellationToken.None);

        Assert.Contains("Código;Produto;Categoria", csv);
        Assert.Contains("\"Cabo; USB\"", csv);
    }

    private static InventoryService CreateService() => new(new MemoryInventoryRepository());

    private static CreateProductRequest NewProduct(
        string sku = "KEYBOARD-01",
        string name = "Produto de teste",
        decimal unitPrice = 100m,
        int minimumStock = 3,
        int maximumStock = 12,
        int? categoryId = null) => new()
        {
            Sku = sku,
            Name = name,
            UnitPrice = unitPrice,
            MinimumStock = minimumStock,
            MaximumStock = maximumStock,
            CategoryId = categoryId
        };

    private static CreateStockMovementRequest NewMovement(
        StockMovementType type,
        int quantity) => new()
        {
            Type = type,
            Quantity = quantity,
            Note = "Movimentação de teste"
        };
}
