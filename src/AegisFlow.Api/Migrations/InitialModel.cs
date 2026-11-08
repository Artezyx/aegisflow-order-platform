using Microsoft.EntityFrameworkCore;

namespace AegisFlow.Api.Migrations;

// Frozen mapping for initial migration. Update via new migrations, never by editing this version.
internal static class InitialModel
{
    internal static void Build(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.Sku).HasMaxLength(80).IsRequired();
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Version).IsConcurrencyToken();
            e.ToTable(t => t.HasCheckConstraint("CK_Products_Available", "[Available] >= 0"));
        });
        modelBuilder.Entity<PurchaseOrder>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.IdempotencyKey).HasMaxLength(128).IsRequired();
            e.Property(o => o.RequestDigest).HasMaxLength(64).IsRequired();
            e.HasIndex(o => o.IdempotencyKey).IsUnique();
            e.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<PurchaseOrderLine>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasIndex(l => l.PurchaseOrderId);
            e.HasOne<Product>().WithMany().HasForeignKey(l => l.ProductId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<StockMovement>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.PurchaseOrderId);
            e.HasOne<Product>().WithMany().HasForeignKey(m => m.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PurchaseOrder>().WithMany().HasForeignKey(m => m.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
