using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options) { }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CustomerName)
                      .IsRequired()
                      .HasMaxLength(200); // best practice to set length

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasMaxLength(50); // status often stored as string

                entity.Property(e => e.CreatedAt)
                      .IsRequired();

                // Configure OrderLine as Owned Collection
                entity.OwnsMany(o => o.Lines, b =>
                {
                    b.WithOwner().HasForeignKey("OrderId");

                    b.Property<int>("Id"); // Shadow property if not mapped
                    b.HasKey("Id", "OrderId"); // Composite key

                    b.Property(l => l.Product)
                        .IsRequired()
                        .HasMaxLength(200);

                    b.Property(l => l.Quantity).IsRequired();

                    // Price as Value Object
                    b.OwnsOne(l => l.Price, m =>
                    {
                        m.Property(p => p.Amount)
                            .HasColumnName("Amount")
                            .HasColumnType("decimal(18,2)");

                        m.Property(p => p.Currency)
                            .HasColumnName("Currency")
                            .HasMaxLength(10);
                    });

                    // Optional: table mapping for clarity
                    b.ToTable("OrderLines");
                });
            });
        }
    }
}
