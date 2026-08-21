using System.ComponentModel.DataAnnotations;

namespace StockFlow.Api.Dtos;

public sealed class CreateCategoryRequest
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;
}

public sealed record CategoryResponse(
    int Id,
    string Name,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
