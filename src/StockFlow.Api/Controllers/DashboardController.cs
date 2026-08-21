using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Dtos;
using StockFlow.Api.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<InventorySummaryResponse>> GetSummary(
        CancellationToken cancellationToken) =>
        Ok(await inventoryService.GetSummaryAsync(cancellationToken));
}
