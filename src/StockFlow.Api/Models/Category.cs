using StockFlow.Api.Domain.Exceptions;

namespace StockFlow.Api.Models;

public sealed class Category
{
    private Category()
    {
    }

    public Category(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O nome da categoria é obrigatório.");
        }

        Name = name.Trim();
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    internal void AssignId(int id)
    {
        if (Id == 0)
        {
            Id = id;
        }
    }
}
