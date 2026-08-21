using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Dtos;
using StockFlow.Api.Security;
using StockFlow.Api.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/suppliers")]
public sealed class SuppliersController(ICatalogService catalogService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SupplierResponse>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await catalogService.GetSuppliersAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = UserRoles.Manager)]
    public async Task<ActionResult<SupplierResponse>> Create(
        CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var supplier = await catalogService.CreateSupplierAsync(request, cancellationToken);
        return Created($"/api/suppliers/{supplier.Id}", supplier);
    }
}
