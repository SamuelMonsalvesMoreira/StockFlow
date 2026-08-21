namespace StockFlow.Api.Configuration;

public sealed class DemoAccountsOptions
{
    public const string SectionName = "DemoAccounts";

    public DemoAccountOptions Manager { get; init; } = new();
    public DemoAccountOptions Viewer { get; init; } = new();
}

public sealed class DemoAccountOptions
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
