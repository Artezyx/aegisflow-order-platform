using System.Xml.Linq;
using AegisFlow.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection must be configured.");
builder.Services.AddDbContext<AegisDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<OrderService>();
builder.Services.AddOrderApiAuthentication(builder.Configuration);
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AegisDbContext>();
    await db.Database.MigrateAsync();
}

app.MapGet("/health", () => Results.Ok(new { service = "AegisFlow", status = "healthy" }));

app.MapGet("/api/products", async (AegisDbContext db, CancellationToken ct) =>
    Results.Ok(await db.Products.AsNoTracking().OrderBy(x => x.Sku).ToListAsync(ct)))
    .RequireAuthorization("inventory.read");

app.MapGet("/api/products/{id:guid}", async (Guid id, AegisDbContext db, CancellationToken ct) =>
{
    var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, ct);
    return product is null ? Results.NotFound() : Results.Ok(product);
}).RequireAuthorization("inventory.read");

app.MapPost("/api/products", async (ProductInput input, AegisDbContext db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(input.Sku) || input.Sku.Length > 80 ||
        string.IsNullOrWhiteSpace(input.Name) || input.Name.Length > 200 || input.Available < 0)
        return Results.BadRequest(new { error = "Invalid SKU, product name or available quantity." });
    var product = new Product { Sku = input.Sku.Trim(), Name = input.Name.Trim(), Available = input.Available };
    db.Products.Add(product);
    try { await db.SaveChangesAsync(ct); }
    catch (DbUpdateException) { return Results.Conflict(new { error = "SKU already exists or violates a constraint." }); }
    return Results.Created($"/api/products/{product.Id}", product);
}).RequireAuthorization("inventory.write");

app.MapPost("/api/orders", async (HttpRequest request, CreateOrderInput input, OrderService service, CancellationToken ct) =>
{
    var key = request.Headers["Idempotency-Key"].ToString();
    try
    {
        var placed = await service.PlaceAsync(input, key, ct);
        return placed.Replayed
            ? Results.Ok(placed)
            : Results.Created($"/api/orders/{placed.Id}", placed);
    }
    catch (DomainFailure failure)
    {
        return Results.Problem(failure.Message, statusCode: failure.StatusCode);
    }
}).RequireAuthorization("orders.write");

app.MapGet("/api/orders/{id:guid}", async (Guid id, AegisDbContext db, CancellationToken ct) =>
{
    var order = await db.Orders.Include(o => o.Lines).AsNoTracking()
        .SingleOrDefaultAsync(o => o.Id == id, ct);
    if (order is null) return Results.NotFound();
    return Results.Ok(new OrderOutcome(order.Id, order.CreatedAt,
        order.Lines.Select(l => new LineInput(l.ProductId, l.Quantity)).ToArray(), false));
}).RequireAuthorization("orders.read");

app.MapGet("/api/orders/{id:guid}/receipt.xml", async (Guid id, AegisDbContext db, CancellationToken ct) =>
{
    var order = await db.Orders.Include(o => o.Lines).AsNoTracking()
        .SingleOrDefaultAsync(o => o.Id == id, ct);
    if (order is null) return Results.NotFound();
    var xml = new XDocument(new XElement("receipt",
        new XAttribute("orderId", order.Id), new XAttribute("createdAt", order.CreatedAt),
        order.Lines.Select(l => new XElement("item",
            new XAttribute("productId", l.ProductId), new XAttribute("quantity", l.Quantity)))));
    return Results.Text(xml.ToString(), "application/xml");
}).RequireAuthorization("orders.read");

app.Run();
