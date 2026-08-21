using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using StockFlow.Api.Configuration;
using StockFlow.Api.Security;
using StockFlow.Api.Services;

namespace StockFlow.Api.Tests;

public sealed class DemoAuthServiceTests
{
    [Theory]
    [InlineData("gestor@stockflow.local", "Gestor123!", UserRoles.Manager)]
    [InlineData("visitante@stockflow.local", "Visitante123!", UserRoles.Viewer)]
    public void Authenticate_WithValidCredentials_ReturnsExpectedRole(
        string email,
        string password,
        string expectedRole)
    {
        var service = CreateService();

        var user = service.Authenticate(email, password);

        Assert.NotNull(user);
        Assert.Equal(expectedRole, user.Role);
        Assert.NotEqual(password, user.PasswordHash);
    }

    [Fact]
    public void Authenticate_WithWrongPassword_ReturnsNull()
    {
        var service = CreateService();

        var user = service.Authenticate("gestor@stockflow.local", "SenhaIncorreta123!");

        Assert.Null(user);
    }

    [Fact]
    public void Authenticate_TreatsEmailAsCaseInsensitive()
    {
        var service = CreateService();

        var user = service.Authenticate("GESTOR@STOCKFLOW.LOCAL", "Gestor123!");

        Assert.NotNull(user);
        Assert.Equal(UserRoles.Manager, user.Role);
    }

    private static DemoAuthService CreateService()
    {
        var options = Options.Create(new DemoAccountsOptions
        {
            Manager = new DemoAccountOptions
            {
                Name = "Gestor de Estoque",
                Email = "gestor@stockflow.local",
                Password = "Gestor123!"
            },
            Viewer = new DemoAccountOptions
            {
                Name = "Visitante",
                Email = "visitante@stockflow.local",
                Password = "Visitante123!"
            }
        });

        return new DemoAuthService(options, new PasswordHasher<AuthenticatedUser>());
    }
}
