using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockFlow.Api.Configuration;
using StockFlow.Api.Data;
using StockFlow.Api.Infrastructure;
using StockFlow.Api.Repositories;
using StockFlow.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var dataProtectionKeysPath = builder.Configuration["DataProtectionKeysPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, ".data-protection-keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .SetApplicationName("StockFlow")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "StockFlow.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.Configure<DemoAccountsOptions>(
    builder.Configuration.GetSection(DemoAccountsOptions.SectionName));
builder.Services.AddSingleton<IPasswordHasher<AuthenticatedUser>, PasswordHasher<AuthenticatedUser>>();
builder.Services.AddSingleton<IAuthService, DemoAuthService>();

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var storageProvider = builder.Configuration["StorageProvider"] ?? "Memory";
var useSqlServer = storageProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

if (useSqlServer)
{
    var connectionString = builder.Configuration.GetConnectionString("SqlServer")
        ?? throw new InvalidOperationException("A conexão 'SqlServer' não foi configurada.");

    builder.Services.AddDbContext<StockFlowDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddScoped<IInventoryRepository, EfInventoryRepository>();
}
else if (storageProvider.Equals("Memory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<IInventoryRepository, MemoryInventoryRepository>();
}
else
{
    throw new InvalidOperationException(
        $"O armazenamento '{storageProvider}' não é suportado. Use 'Memory' ou 'SqlServer'.");
}

builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<DemoInventorySeeder>();

var app = builder.Build();

if (useSqlServer)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<StockFlowDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (builder.Configuration.GetValue("SeedDemoData", false))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DemoInventorySeeder>();
    await seeder.SeedAsync();
}

app.UseExceptionHandler();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapGet("/api/about", () => Results.Ok(new
{
    application = "StockFlow API",
    version = "4.0",
    storage = storageProvider
}));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Recurso não encontrado",
            status = StatusCodes.Status404NotFound
        });
        return;
    }

    var indexPath = Path.Combine(
        app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"),
        "index.html");

    if (!File.Exists(indexPath))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync(
            "A interface ainda não foi compilada. Execute o Angular separadamente durante o desenvolvimento.");
        return;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.SendFileAsync(indexPath);
});

app.Run();

public partial class Program;
