namespace Infrastructure.Base;

using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

public class Context : DbContext
{
    private bool IsTest = false; 
    public Context(bool test = false)
    {
        this.IsTest = test;
        Database.EnsureCreated();
        ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (IsTest)
        {
            optionsBuilder
                .UseNpgsql("Host=127.0.0.1;Port=5432;Database=InventoryControlTest;Username=postgres;Password=MySupperPassword;")
                .UseSnakeCaseNamingConvention();
        }
        else
        { 
            optionsBuilder
                .UseNpgsql("Host=127.0.0.1;Port=5432;Database=InventoryControl;Username=postgres;Password=MySupperPassword;")
                .UseSnakeCaseNamingConvention();
        }
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
