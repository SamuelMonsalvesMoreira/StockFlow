using StockFlow.Api.Domain.Exceptions;

namespace StockFlow.Api.Models;

public sealed class Supplier
{
    private Supplier()
    {
    }

    public Supplier(string name, string? contactName, string? email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessRuleException("O nome do fornecedor é obrigatório.");
        }

        Name = name.Trim();
        ContactName = Clean(contactName);
        Email = Clean(email);
        Phone = Clean(phone);
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? ContactName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    internal void AssignId(int id)
    {
        if (Id == 0)
        {
            Id = id;
        }
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
