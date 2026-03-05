using ArenaBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace ArenaBackend.Data
{
    public class ArenaDbContext : DbContext
    {
        public ArenaDbContext(DbContextOptions<ArenaDbContext> options) : base(options) { }

        // Existing
        public DbSet<Student> Students { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        // Phase 2 - Tabs
        public DbSet<Tab> Tabs { get; set; }
        public DbSet<TabItem> TabItems { get; set; }

        // Phase 2 - Cash Register
        public DbSet<CashRegister> CashRegisters { get; set; }
        public DbSet<CashTransaction> CashTransactions { get; set; }

        // Phase 2 - Purchase Orders
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Sale relationships
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(si => si.SaleId);

            modelBuilder.Entity<SaleItem>()
                .HasOne(si => si.Product)
                .WithMany()
                .HasForeignKey(si => si.ProductId);

            // Tab relationships
            modelBuilder.Entity<Tab>()
                .HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Tab>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Tab>()
                .HasOne(t => t.Sale)
                .WithMany()
                .HasForeignKey(t => t.SaleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TabItem>()
                .HasOne(ti => ti.Tab)
                .WithMany(t => t.Items)
                .HasForeignKey(ti => ti.TabId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TabItem>()
                .HasOne(ti => ti.Product)
                .WithMany()
                .HasForeignKey(ti => ti.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // CashRegister relationships
            modelBuilder.Entity<CashTransaction>()
                .HasOne(ct => ct.CashRegister)
                .WithMany(cr => cr.Transactions)
                .HasForeignKey(ct => ct.CashRegisterId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CashTransaction>()
                .HasOne(ct => ct.Sale)
                .WithMany()
                .HasForeignKey(ct => ct.SaleId)
                .OnDelete(DeleteBehavior.SetNull);

            // PurchaseOrder relationships
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.PurchaseOrder)
                .WithMany(po => po.Items)
                .HasForeignKey(poi => poi.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrderItem>()
                .HasOne(poi => poi.Product)
                .WithMany()
                .HasForeignKey(poi => poi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
