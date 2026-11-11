using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infraestructure.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<BranchStock> BranchStocks { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseDetail> PurchaseDetails { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Cuit);
                entity.Property(u => u.Cuit).HasMaxLength(13);
                entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
                entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(10);
                entity.Property(u => u.IsActive).HasDefaultValue(true);

                // Employee -> una única sucursal
                entity.HasOne<Branch>()
                    .WithMany()
                    .HasForeignKey(u => u.BranchId)
                    .IsRequired(false);

                // Owner -> muchas empresas / empresa -> muchos dueños (EF crea la tabla puente)
                entity.HasMany(u => u.Companies)
                    .WithMany(c => c.Owners)
                    .UsingEntity("CompanyOwners");

                // Sucursal obligatoria si es Employee, nula si es Owner
                entity.ToTable(t => t.HasCheckConstraint("CK_Users_RoleBranch",
                    "(Role = 'Owner' AND BranchId IS NULL) OR (Role = 'Employee' AND BranchId IS NOT NULL)"));
            });

            // Empresa
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
                entity.Property(c => c.IsActive).HasDefaultValue(true);
            });

            // SucursalEmpresa
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Locality).HasMaxLength(100).IsRequired();
                entity.Property(b => b.Address).HasMaxLength(150).IsRequired();
                entity.Property(b => b.Industry).HasConversion<string>();
                entity.Property(b => b.IsActive).HasDefaultValue(true);

                entity.HasOne<Company>()
                    .WithMany()
                    .HasForeignKey(b => b.CompanyId);
            });

            // Producto
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Brand).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Category).HasMaxLength(50).IsRequired();
                entity.Property(p => p.Price).HasPrecision(10, 2);
                entity.Property(p => p.Industry).HasConversion<string>();

                entity.ToTable(t => t.HasCheckConstraint("CK_Products_Price", "Price >= 0"));
            });

            // StockSucursal
            modelBuilder.Entity<BranchStock>(entity =>
            {
                entity.HasKey(s => new { s.BranchId, s.ProductId });
                entity.Property(s => s.IsActive).HasDefaultValue(true);

                entity.HasOne<Branch>()
                    .WithMany()
                    .HasForeignKey(s => s.BranchId);

                entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(s => s.ProductId);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_BranchStocks_Quantity", "Quantity >= 0");
                    t.HasCheckConstraint("CK_BranchStocks_MinimumStock", "MinimumStock >= 0");
                });
            });

            // Venta (el precio unitario queda guardado en el detalle)
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.Number).IsUnique();
                entity.Property(s => s.UserCuit).HasMaxLength(13).IsRequired();
                entity.Property(s => s.Total).HasPrecision(12, 2);

                entity.HasOne<Branch>()
                    .WithMany()
                    .HasForeignKey(s => s.BranchId);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(s => s.UserCuit);

                entity.HasMany(s => s.Details)
                    .WithOne()
                    .HasForeignKey(d => d.SaleId);
            });

            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.HasKey(d => new { d.SaleId, d.ProductId });
                entity.Property(d => d.UnitPrice).HasPrecision(10, 2);

                entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(d => d.ProductId);

                entity.ToTable(t => t.HasCheckConstraint("CK_SaleDetails_Quantity", "Quantity > 0"));
            });

            // Compra de mercadería
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Number).IsUnique();
                entity.Property(p => p.UserCuit).HasMaxLength(13).IsRequired();
                entity.Property(p => p.Supplier).HasMaxLength(100);
                entity.Property(p => p.Total).HasPrecision(12, 2);

                entity.HasOne<Branch>()
                    .WithMany()
                    .HasForeignKey(p => p.BranchId);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(p => p.UserCuit);

                entity.HasMany(p => p.Details)
                    .WithOne()
                    .HasForeignKey(d => d.PurchaseId);
            });

            modelBuilder.Entity<PurchaseDetail>(entity =>
            {
                entity.HasKey(d => new { d.PurchaseId, d.ProductId });
                entity.Property(d => d.UnitCost).HasPrecision(10, 2);

                entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(d => d.ProductId);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_PurchaseDetails_Quantity", "Quantity > 0");
                    t.HasCheckConstraint("CK_PurchaseDetails_UnitCost", "UnitCost >= 0");
                });
            });
        }
    }
}
