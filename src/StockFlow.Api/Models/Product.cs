using StockFlow.Api.Domain.Exceptions;

namespace StockFlow.Api.Models;

public sealed class Product
{
    private Product()
    {
    }

    public Product(
        string sku,
        string name,
        decimal unitPrice,
        int minimumStock,
        int maximumStock,
        Category? category,
        Supplier? supplier)
    {
        ValidateSku(sku);
        ValidateDetails(name, unitPrice, minimumStock, maximumStock);

        Sku = sku.Trim().ToUpperInvariant();
        Name = name.Trim();
        UnitPrice = unitPrice;
        MinimumStock = minimumStock;
        MaximumStock = maximumStock;
        SetCatalogReferences(category, supplier);
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int QuantityInStock { get; private set; }
    public int MinimumStock { get; private set; }
    public int MaximumStock { get; private set; }
    public int? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public int? SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public bool IsLowStock => QuantityInStock <= MinimumStock;
    public int SuggestedReorderQuantity =>
        IsLowStock ? Math.Max(0, MaximumStock - QuantityInStock) : 0;
    public decimal StockValue => QuantityInStock * UnitPrice;

    public void UpdateDetails(
        string name,
        decimal unitPrice,
        int minimumStock,
        int maximumStock,
        Category? category,
        Supplier? supplier)
    {
        ValidateDetails(name, unitPrice, minimumStock, maximumStock);

        Name = name.Trim();
        UnitPrice = unitPrice;
        MinimumStock = minimumStock;
        MaximumStock = maximumStock;
        SetCatalogReferences(category, supplier);
    }

    public StockMovement RegisterMovement(
        StockMovementType type,
        int quantity,
        string? note,
        string performedByName,
        string performedByEmail)
    {
        if (quantity <= 0)
        {
            throw new BusinessRuleException("A quantidade da movimentação deve ser maior que zero.");
        }

        if (string.IsNullOrWhiteSpace(performedByName)
            || string.IsNullOrWhiteSpace(performedByEmail))
        {
            throw new BusinessRuleException("O responsável pela movimentação é obrigatório.");
        }

        var resultingBalance = type switch
        {
            StockMovementType.Entry => checked(QuantityInStock + quantity),
            StockMovementType.Exit when quantity <= QuantityInStock => QuantityInStock - quantity,
            StockMovementType.Exit => throw new BusinessRuleException(
                $"Saldo insuficiente. Disponível: {QuantityInStock}; solicitado: {quantity}."),
            _ => throw new BusinessRuleException("O tipo de movimentação é inválido.")
        };

        QuantityInStock = resultingBalance;

        return new StockMovement(
            Id,
            type,
            quantity,
            resultingBalance,
            note?.Trim(),
            performedByName.Trim(),
            performedByEmail.Trim().ToLowerInvariant());
    }

    internal void AssignId(int id)
    {
        if (Id == 0)
        {
            Id = id;
        }
    }

    private static void ValidateSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new BusinessRuleException("O código do produto (SKU) é obrigatório.");
        }
    }

    private static void ValidateDetails(
        string name,
        decimal unitPrice,
        int minimumStock,
        int maximumStock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O nome do produto é obrigatório.");
        }

        if (unitPrice <= 0)
        {
            throw new BusinessRuleException("O preço unitário deve ser maior que zero.");
        }

        if (minimumStock < 0)
        {
            throw new BusinessRuleException("O estoque mínimo não pode ser negativo.");
        }

        if (maximumStock < minimumStock)
        {
            throw new BusinessRuleException(
                "O estoque máximo deve ser maior ou igual ao estoque mínimo.");
        }
    }

    private void SetCatalogReferences(Category? category, Supplier? supplier)
    {
        Category = category;
        CategoryId = category?.Id;
        Supplier = supplier;
        SupplierId = supplier?.Id;
    }
}
