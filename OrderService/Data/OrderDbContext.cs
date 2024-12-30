using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    public class OrderDbContext(DbContextOptions<OrderDbContext> options, ITenantContext tenantContext) : DbContext(options)
    {
        private ITenantContext _tenantContext = tenantContext;
        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(_tenantContext.ConnectionString))
            {
                optionsBuilder.UseNpgsql(_tenantContext.ConnectionString);
            }
            else
            {
                throw new ArgumentNullException(_tenantContext.ConnectionString, "Connection string not set for tenant.");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().HasData(
            new Order
            {
                OrderId = 1,
                OrderNumber = "132456",
                Customer = "customer a",
            },
            new Order
            {
                OrderId = 2,
                OrderNumber = "789123",
                Customer = "customer b",
            },
            new Order
            {
                OrderId = 3,
                OrderNumber = "456789",
                Customer = "customer c",
            }
        );
        }
    }
}
