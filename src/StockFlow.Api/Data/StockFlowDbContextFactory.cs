using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StockFlow.Api.Data;

public sealed class StockFlowDbContextFactory : IDesignTimeDbContextFactory<StockFlowDbContext>
{
    public StockFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=StockFlowDb;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<StockFlowDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new StockFlowDbContext(options);
    }
}
