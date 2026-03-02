namespace Infrastructure.Base;

using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

public class Context : DbContext
{
    public Context(DbContextOptions options) : base(options)
    {
        Database.EnsureCreated();
        ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSnakeCaseNamingConvention();
    }

    public DbSet<Client> Clients { get; set; }
    public DbSet<MeasureUnit> MeasureUnits { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<ReceiptItem> ReceiptItems { get; set; }
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ShipmentItem> ShipmentItems { get; set; }
}
