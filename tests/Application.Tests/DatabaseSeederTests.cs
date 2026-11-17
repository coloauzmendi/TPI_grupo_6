using System.Text.RegularExpressions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests
{
    public class DatabaseSeederTests : IDisposable
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        public DatabaseSeederTests()
        {
            _connection.Open();
            using ApplicationContext context = NewContext();
            context.Database.EnsureCreated();
        }

        public void Dispose() => _connection.Dispose();

        [Fact]
        public void Seed_EjecutadoDosVeces_NoDuplicaDatos()
        {
            bool first = NewSeeder(NewContext()).Seed();
            int[] countsAfterFirst = Counts(NewContext());

            bool second = NewSeeder(NewContext()).Seed();

            Assert.True(first);
            Assert.False(second);
            Assert.Equal(countsAfterFirst, Counts(NewContext()));
        }

        [Fact]
        public void Seed_ConDatosExistentes_NoModificaNada()
        {
            using (ApplicationContext context = NewContext())
            {
                context.Companies.Add(new Company { Id = Guid.NewGuid(), Name = "Existente" });
                context.SaveChanges();
            }

            Assert.False(NewSeeder(NewContext()).Seed());
            Assert.Equal(1, NewContext().Companies.Count());
            Assert.Empty(NewContext().Users);
        }

        [Fact]
        public void Seed_DatosCargados_RespetanLasReglasDeNegocio()
        {
            NewSeeder(NewContext()).Seed();
            using ApplicationContext context = NewContext();

            List<User> users = context.Users.Include(u => u.Companies).ToList();
            List<Branch> branches = context.Branches.ToList();
            List<Product> products = context.Products.ToList();
            List<BranchStock> stocks = context.BranchStocks.ToList();
            List<Sale> sales = context.Sales.Include(s => s.Details).ToList();
            List<Purchase> purchases = context.Purchases.Include(p => p.Details).ToList();
            PasswordHashService hashService = new();

            Assert.All(users, u => Assert.Matches(new Regex(@"^\d{2}-\d{8}-\d$"), u.Cuit));
            Assert.All(users, u => Assert.True(hashService.Verify(u,
                u.Role == UserRole.Owner ? SeedScenario.OwnerPassword : SeedScenario.EmployeePassword)));
            Assert.All(users, u => Assert.NotEqual(SeedScenario.OwnerPassword, u.PasswordHash));
            Assert.Contains(users, u => u.Role == UserRole.Owner && u.Companies.Count > 1);
            Assert.Contains(users, u => u.Role == UserRole.Owner && u.Companies.Count == 1);
            Assert.All(users.Where(u => u.Role == UserRole.Employee), u => Assert.Contains(branches, b => b.Id == u.BranchId));

            Assert.InRange(context.Companies.Count(), 2, 3);
            Assert.All(context.Companies.ToList(), c => Assert.True(branches.Count(b => b.CompanyId == c.Id) > 1));

            Assert.Equal(Enumerable.Range(1, sales.Count), sales.OrderBy(s => s.Date).Select(s => s.Number));
            Assert.Equal(Enumerable.Range(1, purchases.Count), purchases.OrderBy(p => p.Date).ThenBy(p => p.Number).Select(p => p.Number));
            Assert.All(sales, s => Assert.Equal(s.Details.Sum(d => d.Quantity * d.UnitPrice), s.Total));
            Assert.All(purchases, p => Assert.Equal(p.Details.Sum(d => d.Quantity * d.UnitCost), p.Total));
            Assert.All(sales, s => Assert.True(s.Date <= DateTime.Now));

            Dictionary<Guid, Guid> branchCompany = branches.ToDictionary(b => b.Id, b => b.CompanyId);
            Assert.All(stocks, s => Assert.Equal(branchCompany[s.BranchId], products.Single(p => p.Id == s.ProductId).CompanyId));
            Assert.All(stocks, s =>
            {
                int purchased = purchases.Where(p => p.BranchId == s.BranchId)
                    .SelectMany(p => p.Details).Where(d => d.ProductId == s.ProductId).Sum(d => d.Quantity);
                int sold = sales.Where(v => v.BranchId == s.BranchId && !v.IsCancelled)
                    .SelectMany(v => v.Details).Where(d => d.ProductId == s.ProductId).Sum(d => d.Quantity);

                Assert.True(s.Quantity >= 0);
                Assert.Equal(purchased - sold, s.Quantity);
            });
            Assert.All(products.Where(p => !p.IsActive), p => Assert.All(stocks.Where(s => s.ProductId == p.Id), s => Assert.False(s.IsActive)));

            Assert.Contains(sales, s => s.IsCancelled);
            Assert.Contains(sales, s => users.Single(u => u.Cuit == s.UserCuit).Role == UserRole.Owner);
            Assert.Contains(sales, s => users.Single(u => u.Cuit == s.UserCuit).Role == UserRole.Employee);
            Assert.True(sales.Select(s => s.Date.Date).Distinct().Count() > 1);
            Assert.Contains(purchases, p => p.Supplier == Application.Services.StockService.InitialLoadSupplier);
            Assert.Contains(stocks, s => s.IsActive && s.IsLow);
            Assert.Contains(stocks, s => s.Quantity == 0);
            Assert.Contains(products, p => !p.IsActive);
            Assert.Contains(products, p => p.IsActive && stocks.All(s => s.ProductId != p.Id));
        }

        private ApplicationContext NewContext()
        {
            return new ApplicationContext(new DbContextOptionsBuilder<ApplicationContext>().UseSqlite(_connection).Options);
        }

        private static DatabaseSeeder NewSeeder(ApplicationContext context)
        {
            return new DatabaseSeeder(context, new PasswordHashService());
        }

        private static int[] Counts(ApplicationContext context)
        {
            return new[]
            {
                context.Users.Count(), context.Companies.Count(), context.Branches.Count(), context.Products.Count(),
                context.BranchStocks.Count(), context.Purchases.Count(), context.PurchaseDetails.Count(),
                context.Sales.Count(), context.SaleDetails.Count()
            };
        }
    }
}
