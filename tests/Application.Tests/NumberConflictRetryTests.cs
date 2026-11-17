using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Application.Tests
{
    // Simula otra operación que toma el mismo número justo antes de guardar, contra una base SQLite en memoria
    public class NumberConflictRetryTests : IDisposable
    {
        private const string Cuit = "20111111112";

        private readonly SqliteConnection _connection = new("DataSource=:memory:");
        private readonly Guid _branchId = Guid.NewGuid();

        public NumberConflictRetryTests()
        {
            _connection.Open();
            using ApplicationContext context = NewContext();
            context.Database.EnsureCreated();

            Company company = new Company { Id = Guid.NewGuid(), Name = "Kiosco" };
            context.Companies.Add(company);
            context.Branches.Add(new Branch { Id = _branchId, CompanyId = company.Id, Locality = "Rosario", Industry = Industry.Kiosk });
            context.Users.Add(new User { Cuit = Cuit, FirstName = "Ana", LastName = "Perez", Role = UserRole.Owner });
            context.SaveChanges();
        }

        public void Dispose() => _connection.Dispose();

        [Fact]
        public void SaleCreate_NumeroTomadoPorOtraVenta_ReintentaConElSiguiente()
        {
            using ApplicationContext context = NewContext(new ConflictingInsert(this, times: 1, insertSale: true));

            Sale sale = new SaleRepository(context).Create(NewSale(), new List<BranchStock>());

            Assert.Equal(2, sale.Number);
            Assert.Equal(new[] { 1, 2 }, NewContext().Sales.Select(s => s.Number).OrderBy(n => n).ToArray());
        }

        [Fact]
        public void PurchaseCreate_NumeroTomadoPorOtraCompra_ReintentaConElSiguiente()
        {
            using ApplicationContext context = NewContext(new ConflictingInsert(this, times: 1, insertSale: false));

            Purchase purchase = new PurchaseRepository(context).Create(NewPurchase(), new List<BranchStock>());

            Assert.Equal(2, purchase.Number);
            Assert.Equal(2, NewContext().Purchases.Count());
        }

        [Fact]
        public void SaleCreate_ConflictoEnElReintento_NoReintentaDeNuevo()
        {
            using ApplicationContext context = NewContext(new ConflictingInsert(this, times: 2, insertSale: true));

            Assert.Throws<DbUpdateException>(() => new SaleRepository(context).Create(NewSale(), new List<BranchStock>()));
            Assert.Equal(2, NewContext().Sales.Count());
        }

        private ApplicationContext NewContext(IInterceptor? interceptor = null)
        {
            DbContextOptionsBuilder<ApplicationContext> builder = new DbContextOptionsBuilder<ApplicationContext>().UseSqlite(_connection);
            if (interceptor is not null)
            {
                builder.AddInterceptors(interceptor);
            }

            return new ApplicationContext(builder.Options);
        }

        private Sale NewSale() => new Sale { Id = Guid.NewGuid(), BranchId = _branchId, UserCuit = Cuit, Date = DateTime.Now };

        private Purchase NewPurchase() => new Purchase { Id = Guid.NewGuid(), BranchId = _branchId, UserCuit = Cuit, Date = DateTime.Now };

        // Antes de cada guardado, otra "operación" guarda un comprobante con el número que se está por usar
        private class ConflictingInsert : SaveChangesInterceptor
        {
            private readonly NumberConflictRetryTests _test;
            private readonly bool _insertSale;
            private int _remaining;

            public ConflictingInsert(NumberConflictRetryTests test, int times, bool insertSale)
            {
                _test = test;
                _remaining = times;
                _insertSale = insertSale;
            }

            public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
            {
                if (_remaining-- > 0)
                {
                    using ApplicationContext other = _test.NewContext();
                    if (_insertSale)
                    {
                        Sale sale = _test.NewSale();
                        sale.Number = (other.Sales.Max(s => (int?)s.Number) ?? 0) + 1;
                        other.Sales.Add(sale);
                    }
                    else
                    {
                        Purchase purchase = _test.NewPurchase();
                        purchase.Number = (other.Purchases.Max(p => (int?)p.Number) ?? 0) + 1;
                        other.Purchases.Add(purchase);
                    }

                    other.SaveChanges();
                }

                return result;
            }
        }
    }
}
