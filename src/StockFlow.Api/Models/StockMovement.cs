namespace StockFlow.Api.Models;

public sealed class StockMovement
{
    private StockMovement()
    {
    }

    internal StockMovement(
        int productId,
        StockMovementType type,
        int quantity,
        int resultingBalance,
        string? note,
        string performedByName,
        string performedByEmail)
    {
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        ResultingBalance = resultingBalance;
        Note = note;
        PerformedByName = performedByName;
        PerformedByEmail = performedByEmail;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public StockMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public int ResultingBalance { get; private set; }
    public string? Note { get; private set; }
    public string PerformedByName { get; private set; } = string.Empty;
    public string PerformedByEmail { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    internal void AssignId(int id)
    {
        if (Id == 0)
        {
            Id = id;
        }
    }
}
