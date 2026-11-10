using Microsoft.EntityFrameworkCore;
using ShopApp.Domain;

namespace ShopApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(order =>
        {
            order.ToTable("orders");
            order.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
