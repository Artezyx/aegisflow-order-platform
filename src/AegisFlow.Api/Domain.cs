namespace AegisFlow.Api;

public sealed class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public int Available { get; set; }
    public long Version { get; set; }
}

public sealed class PurchaseOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string IdempotencyKey { get; set; }
    public required string RequestDigest { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<PurchaseOrderLine> Lines { get; set; } = new();
}

public sealed class PurchaseOrderLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class StockMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public int Delta { get; set; }
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed record ProductInput(string Sku, string Name, int Available);
public sealed record LineInput(Guid ProductId, int Quantity);
public sealed record CreateOrderInput(IReadOnlyList<LineInput>? Items);
public sealed record OrderOutcome(Guid Id, DateTimeOffset CreatedAt, IReadOnlyList<LineInput> Items, bool Replayed);
public sealed class DomainFailure(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
