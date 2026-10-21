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
            });

            // SucursalEmpresa
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Locality).HasMaxLength(100).IsRequired();
                entity.Property(b => b.Address).HasMaxLength(150).IsRequired();
                entity.Property(b => b.Industry).HasConversion<string>();

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
        }
    }
}
