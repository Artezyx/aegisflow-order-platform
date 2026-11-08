using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Api;

public sealed class OrderService(AegisDbContext db)
{
    public async Task<OrderOutcome> PlaceAsync(CreateOrderInput input, string key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 128)
            throw new DomainFailure(400, "Idempotency-Key is required (maximum 128 characters).");
        if (input.Items is null || input.Items.Count == 0 ||
            input.Items.Any(x => x.ProductId == Guid.Empty || x.Quantity <= 0))
            throw new DomainFailure(400, "Provide at least one valid product and positive quantity.");

        var lines = input.Items.GroupBy(x => x.ProductId)
            .Select(g => new LineInput(g.Key, checked(g.Sum(x => x.Quantity))))
            .OrderBy(x => x.ProductId).ToArray();
        var signature = string.Join(";", lines.Select(x => $"{x.ProductId:N}:{x.Quantity}"));
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signature)));

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var existing = await db.Orders.Include(o => o.Lines)
            .SingleOrDefaultAsync(o => o.IdempotencyKey == key, ct);
        if (existing is not null)
        {
            if (existing.RequestDigest != digest)
                throw new DomainFailure(409, "This idempotency key belongs to a different request.");
            await transaction.CommitAsync(ct);
            return ToOutcome(existing, true);
        }

        var productIds = lines.Select(x => x.ProductId).ToArray();
        var products = await db.Products.Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);
        if (products.Count != lines.Length)
            throw new DomainFailure(404, "One or more products could not be found.");
        if (lines.Any(l => products[l.ProductId].Available < l.Quantity))
            throw new DomainFailure(409, "Insufficient inventory to fulfill this order.");

        var order = new PurchaseOrder { IdempotencyKey = key, RequestDigest = digest };
        foreach (var line in lines)
        {
            var product = products[line.ProductId];
            product.Available -= line.Quantity;
            product.Version++;
            order.Lines.Add(new PurchaseOrderLine { ProductId = line.ProductId, Quantity = line.Quantity });
            db.Movements.Add(new StockMovement
            {
                ProductId = line.ProductId, PurchaseOrderId = order.Id, Delta = -line.Quantity
            });
        }
        db.Orders.Add(order);
        try
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DomainFailure(409, "Concurrent inventory update. Please retry with the same key.");
        }
        catch (DbUpdateException)
        {
            throw new DomainFailure(409, "Inventory or order conflict. Please retry with the same key.");
        }
        return ToOutcome(order, false);
    }

    private static OrderOutcome ToOutcome(PurchaseOrder order, bool replayed) =>
        new(order.Id, order.CreatedAt,
            order.Lines.Select(l => new LineInput(l.ProductId, l.Quantity)).ToArray(), replayed);
}
