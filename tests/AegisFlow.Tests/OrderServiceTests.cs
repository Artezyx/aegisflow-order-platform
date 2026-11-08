using AegisFlow.Api;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AegisFlow.Tests;

public sealed class OrderServiceTests
{
    private static async Task<(SqliteConnection, AegisDbContext, Product)> SetupAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AegisDbContext>().UseSqlite(connection).Options;
        var db = new AegisDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var product = new Product { Sku = "AX-500", Name = "Controller", Available = 4 };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return (connection, db, product);
    }

    [Fact]
    public async Task Retry_DoesNotReserveInventoryTwice()
    {
        var (connection, db, product) = await SetupAsync();
        await using var conn = connection;
        await using var context = db;
        var service = new OrderService(db);
        var input = new CreateOrderInput(new[] { new LineInput(product.Id, 2) });
        var first = await service.PlaceAsync(input, "request-42");
        var second = await service.PlaceAsync(input, "request-42");
        Assert.Equal(first.Id, second.Id);
        Assert.True(second.Replayed);
        Assert.Equal(2, (await db.Products.SingleAsync()).Available);
        Assert.Single(await db.Orders.ToListAsync());
        Assert.Single(await db.Movements.ToListAsync());
    }

    [Fact]
    public async Task ChangedPayload_WithSameKey_ReturnsConflict()
    {
        var (connection, db, product) = await SetupAsync();
        await using var conn = connection;
        await using var context = db;
        var service = new OrderService(db);
        await service.PlaceAsync(new CreateOrderInput(new[] { new LineInput(product.Id, 1) }), "locked-key");
        var error = await Assert.ThrowsAsync<DomainFailure>(() =>
            service.PlaceAsync(new CreateOrderInput(new[] { new LineInput(product.Id, 2) }), "locked-key"));
        Assert.Equal(409, error.StatusCode);
        Assert.Equal(3, (await db.Products.SingleAsync()).Available);
    }

    [Fact]
    public async Task InsufficientInventory_RollsBackOrderAndLedger()
    {
        var (connection, db, product) = await SetupAsync();
        await using var conn = connection;
        await using var context = db;
        var service = new OrderService(db);
        var error = await Assert.ThrowsAsync<DomainFailure>(() =>
            service.PlaceAsync(new CreateOrderInput(new[] { new LineInput(product.Id, 5) }), "too-many"));
        Assert.Equal(409, error.StatusCode);
        Assert.Empty(await db.Orders.ToListAsync());
        Assert.Empty(await db.Movements.ToListAsync());
        Assert.Equal(4, (await db.Products.SingleAsync()).Available);
    }

    [Fact]
    public async Task VersionToken_RejectsStaleInventoryWriter()
    {
        var (connection, db, product) = await SetupAsync();
        await using var conn = connection;
        await using var first = db;
        var options = new DbContextOptionsBuilder<AegisDbContext>().UseSqlite(connection).Options;
        await using var second = new AegisDbContext(options);
        var fresh = await db.Products.SingleAsync(p => p.Id == product.Id);
        var stale = await second.Products.SingleAsync(p => p.Id == product.Id);
        fresh.Available--;
        fresh.Version++;
        await first.SaveChangesAsync();
        stale.Available--;
        stale.Version++;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        Assert.Equal(3, (await first.Products.AsNoTracking().SingleAsync()).Available);
    }
}
