using System.ComponentModel.DataAnnotations;

namespace StockFlow.Api.Dtos;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;
}

public sealed record UserSessionResponse(
    string Name,
    string Email,
    string Role,
    bool CanManage);
