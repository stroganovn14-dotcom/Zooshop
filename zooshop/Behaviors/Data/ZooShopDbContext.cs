using Microsoft.EntityFrameworkCore;
using ZooShop.Models;

namespace ZooShop.Data;

/// <summary>
/// Основной контекст EF Core.
/// Connection string задаётся через DI (IConfiguration → appsettings.json).
/// </summary>
public class ZooShopDbContext : DbContext
{
    public DbSet<User>         Users         => Set<User>();
    public DbSet<Product>      Products      => Set<Product>();
    public DbSet<Animal>       Animals       => Set<Animal>();
    public DbSet<Sale>         Sales         => Set<Sale>();
    public DbSet<Delivery>     Deliveries    => Set<Delivery>();
    public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();

    public ZooShopDbContext(DbContextOptions<ZooShopDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Логин уникален
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        // Роль хранится как int для компактности
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<int>();

        // Тип продажи — int
        modelBuilder.Entity<Sale>()
            .Property(s => s.ItemType)
            .HasConversion<int>();

        // Удаление продавца не каскадирует на продажи
        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Seller)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Delivery>()
            .HasOne(d => d.AcceptedBy)
            .WithMany()
            .HasForeignKey(d => d.AcceptedById)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryItem>()
            .HasOne(di => di.Delivery)
            .WithMany(d => d.Items)
            .HasForeignKey(di => di.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeliveryItem>()
            .HasOne(di => di.Product)
            .WithMany()
            .HasForeignKey(di => di.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
