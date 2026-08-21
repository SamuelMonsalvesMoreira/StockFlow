using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Dtos;
using StockFlow.Api.Services;

namespace StockFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<ReportOverviewResponse>> GetOverview(
        CancellationToken cancellationToken) =>
        Ok(await reportService.GetOverviewAsync(cancellationToken));

    [HttpGet("inventory.csv")]
    public async Task<IActionResult> ExportInventory(
        CancellationToken cancellationToken)
    {
        var csv = await reportService.ExportInventoryCsvAsync(cancellationToken);
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var content = encoding.GetPreamble().Concat(encoding.GetBytes(csv)).ToArray();
        var fileName = $"stockflow-inventario-{DateTime.UtcNow:yyyy-MM-dd}.csv";

        return File(content, "text/csv; charset=utf-8", fileName);
    }
}
