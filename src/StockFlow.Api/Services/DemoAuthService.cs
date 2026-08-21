using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StockFlow.Api.Configuration;
using StockFlow.Api.Security;

namespace StockFlow.Api.Services;

public sealed class DemoAuthService : IAuthService
{
    private readonly IPasswordHasher<AuthenticatedUser> passwordHasher;
    private readonly IReadOnlyList<AuthenticatedUser> users;

    public DemoAuthService(
        IOptions<DemoAccountsOptions> options,
        IPasswordHasher<AuthenticatedUser> passwordHasher)
    {
        this.passwordHasher = passwordHasher;
        var configuredAccounts = options.Value;

        users =
        [
            CreateUser(configuredAccounts.Manager, UserRoles.Manager),
            CreateUser(configuredAccounts.Viewer, UserRoles.Viewer)
        ];
    }

    public AuthenticatedUser? Authenticate(string email, string password)
    {
        var normalizedEmail = email.Trim();
        var user = users.FirstOrDefault(candidate =>
            candidate.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

        if (user is null)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded
            ? user
            : null;
    }

    private AuthenticatedUser CreateUser(DemoAccountOptions account, string role)
    {
        if (string.IsNullOrWhiteSpace(account.Name)
            || string.IsNullOrWhiteSpace(account.Email)
            || string.IsNullOrWhiteSpace(account.Password))
        {
            throw new InvalidOperationException(
                $"A conta de demonstração do perfil {role} não foi configurada corretamente.");
        }

        var userWithoutPassword = new AuthenticatedUser(
            account.Name.Trim(),
            account.Email.Trim().ToLowerInvariant(),
            role,
            string.Empty);
        var hash = passwordHasher.HashPassword(userWithoutPassword, account.Password);

        return userWithoutPassword with { PasswordHash = hash };
    }
}
