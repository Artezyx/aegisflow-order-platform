using Meridian.Api;
using Microsoft.Data.SqlClient;
using System.Data;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(o => o.AddPolicy("local-web", p => p
    .WithOrigins("http://127.0.0.1:5173","http://localhost:5173")
    .WithMethods("GET").AllowAnyHeader()));
var app = builder.Build();
app.UseCors("local-web");
app.MapGet("/health", () => Results.Ok(new { service = "Meridian Insights", status = "ready" }));
app.MapGet("/api/metrics", async (IConfiguration config, CancellationToken ct) =>
{
    var cs = config.GetConnectionString("Meridian");
    if (string.IsNullOrWhiteSpace(cs)) return Results.Problem("SQL Server is not configured.", statusCode: 503);
    await using var db = new SqlConnection(cs);
    await db.OpenAsync(ct);
    const string sql = """
        SELECT COUNT_BIG(*),
          COALESCE(SUM(CASE WHEN Status='Completed' THEN SalesAmount ELSE 0 END),0),
          COALESCE(SUM(CASE WHEN Status='Completed' THEN CostAmount ELSE 0 END),0),
          COALESCE(SUM(CASE WHEN Status='Open' THEN SalesAmount ELSE 0 END),0)
        FROM dbo.FactSales
        """;
    await using var command = new SqlCommand(sql, db);
    await using var row = await command.ExecuteReaderAsync(ct);
    await row.ReadAsync(ct);
    var revenue = row.GetDecimal(1);
    var cost = row.GetDecimal(2);
    return Results.Ok(new {
        orders = row.GetInt64(0), revenue, cost,
        marginPercent = Forecast.MarginPercent(revenue, cost),
        openPipeline = row.GetDecimal(3), currency = "MXN"
    });
});
app.MapGet("/api/revenue/daily", async (int? days, IConfiguration config, CancellationToken ct) =>
{
    var period = days ?? 30;
    if (period is < 1 or > 90) return Results.BadRequest(new { error = "days must be 1-90" });
    var cs = config.GetConnectionString("Meridian");
    if (string.IsNullOrWhiteSpace(cs)) return Results.Problem("SQL Server is not configured.", statusCode: 503);
    await using var db = new SqlConnection(cs);
    await db.OpenAsync(ct);
    const string sql = """
        SELECT CAST(OrderDate AS date),SUM(SalesAmount)
        FROM dbo.FactSales
        WHERE Status='Completed' AND OrderDate>=DATEADD(day,-@days,SYSUTCDATETIME())
        GROUP BY CAST(OrderDate AS date) ORDER BY CAST(OrderDate AS date)
        """;
    await using var cmd = new SqlCommand(sql, db);
    cmd.Parameters.Add("@days", SqlDbType.Int).Value = period;
    await using var rows = await cmd.ExecuteReaderAsync(ct);
    var output = new List<object>();
    while (await rows.ReadAsync(ct))
        output.Add(new { date = rows.GetDateTime(0).ToString("yyyy-MM-dd"), revenue = rows.GetDecimal(1) });
    return Results.Ok(output);
});
app.Run();
