using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Dtos;
using StockFlow.Api.Security;
using StockFlow.Api.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool lowStockOnly = false,
        CancellationToken cancellationToken = default) =>
        Ok(await inventoryService.GetProductsAsync(search, lowStockOnly, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await inventoryService.GetProductByIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = UserRoles.Manager)]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await inventoryService.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRoles.Manager)]
    public async Task<ActionResult<ProductResponse>> Update(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken) =>
        Ok(await inventoryService.UpdateProductAsync(id, request, cancellationToken));

    [HttpGet("{id:int}/movements")]
    public async Task<ActionResult<IReadOnlyList<StockMovementResponse>>> GetMovements(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await inventoryService.GetMovementsAsync(id, cancellationToken));

    [HttpPost("{id:int}/movements")]
    [Authorize(Roles = UserRoles.Manager)]
    public async Task<ActionResult<StockMovementResponse>> RegisterMovement(
        int id,
        CreateStockMovementRequest request,
        CancellationToken cancellationToken)
    {
        var performedByName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var performedByEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        return Ok(await inventoryService.RegisterMovementAsync(
            id,
            request,
            performedByName,
            performedByEmail,
            cancellationToken));
    }
}
