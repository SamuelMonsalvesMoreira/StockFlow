using System.Globalization;
using System.Text;
using StockFlow.Api.Dtos;
using StockFlow.Api.Repositories;

namespace StockFlow.Api.Services;

public sealed class ReportService(IInventoryRepository repository) : IReportService
{
    public async Task<ReportOverviewResponse> GetOverviewAsync(
        CancellationToken cancellationToken)
    {
        var products = await repository.GetProductsAsync(
            search: null,
            lowStockOnly: false,
            cancellationToken);
        var movements = await repository.GetAllMovementsAsync(cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);

        var summary = new InventorySummaryResponse(
            products.Count,
            products.Count(product => product.IsLowStock),
            products.Sum(product => product.QuantityInStock),
            products.Sum(product => product.StockValue));

        var categories = products
            .GroupBy(product => product.Category?.Name ?? "Sem categoria")
            .Select(group => new CategoryReportItem(
                group.Key,
                group.Count(),
                group.Sum(product => product.QuantityInStock),
                group.Sum(product => product.StockValue)))
            .OrderByDescending(category => category.TotalStockValue)
            .ThenBy(category => category.CategoryName)
            .ToList();

        var lowStockProducts = products
            .Where(product => product.IsLowStock)
            .OrderByDescending(product => product.SuggestedReorderQuantity)
            .ThenBy(product => product.Name)
            .Select(product => new LowStockReportItem(
                product.Id,
                product.Sku,
                product.Name,
                product.QuantityInStock,
                product.MinimumStock,
                product.MaximumStock,
                product.SuggestedReorderQuantity))
            .ToList();

        var recentMovements = movements
            .Where(movement => productsById.ContainsKey(movement.ProductId))
            .Take(20)
            .Select(movement =>
            {
                var product = productsById[movement.ProductId];
                return new MovementReportItem(
                    movement.Id,
                    movement.ProductId,
                    product.Sku,
                    product.Name,
                    movement.Type,
                    movement.Quantity,
                    movement.ResultingBalance,
                    movement.PerformedByName,
                    movement.CreatedAtUtc);
            })
            .ToList();

        return new ReportOverviewResponse(
            DateTimeOffset.UtcNow,
            summary,
            categories,
            lowStockProducts,
            recentMovements);
    }

    public async Task<string> ExportInventoryCsvAsync(
        CancellationToken cancellationToken)
    {
        var products = await repository.GetProductsAsync(
            search: null,
            lowStockOnly: false,
            cancellationToken);
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        var csv = new StringBuilder();

        csv.AppendLine(
            "Código;Produto;Categoria;Fornecedor;Saldo;Estoque mínimo;Estoque máximo;Reposição sugerida;Preço unitário;Valor em estoque;Status");

        foreach (var product in products.OrderBy(product => product.Name))
        {
            var values = new[]
            {
                product.Sku,
                product.Name,
                product.Category?.Name ?? "Sem categoria",
                product.Supplier?.Name ?? "Sem fornecedor",
                product.QuantityInStock.ToString(culture),
                product.MinimumStock.ToString(culture),
                product.MaximumStock.ToString(culture),
                product.SuggestedReorderQuantity.ToString(culture),
                product.UnitPrice.ToString("F2", culture),
                product.StockValue.ToString("F2", culture),
                product.IsLowStock ? "Estoque baixo" : "Disponível"
            };

            csv.AppendLine(string.Join(';', values.Select(EscapeCsvValue)));
        }

        return csv.ToString();
    }

    private static string EscapeCsvValue(string value)
    {
        if (!value.Contains(';') && !value.Contains('"')
            && !value.Contains('\r') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
