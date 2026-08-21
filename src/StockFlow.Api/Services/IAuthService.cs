namespace StockFlow.Api.Services;

public interface IAuthService
{
    AuthenticatedUser? Authenticate(string email, string password);
}

public sealed record AuthenticatedUser(
    string Name,
    string Email,
    string Role,
    string PasswordHash);
