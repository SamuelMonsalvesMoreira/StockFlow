using StockFlow.Api.Dtos;

namespace StockFlow.Api.Services;

public interface IReportService
{
    Task<ReportOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken);

    Task<string> ExportInventoryCsvAsync(CancellationToken cancellationToken);
}
