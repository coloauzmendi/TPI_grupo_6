using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Tests.Fakes
{
    // Repositorios en memoria: reemplazan a los de Infrastructure para probar los servicios sin base de datos

    // Mismo comportamiento que Repository<T>: baja lógica si la entidad la admite; si no, se elimina
    public class FakeRepository<T> : IRepository<T> where T : class
    {
        protected List<T> Items { get; } = new();

        public T Create(T entity) { Items.Add(entity); return entity; }
        public void Update(T entity) { }
        public void Delete(T entity)
        {
            if (entity is ISoftDelete softDeletable)
            {
                softDeletable.IsActive = false;
            }
            else
            {
                Items.Remove(entity);
            }
        }
    }

    public class FakeUserRepository : FakeRepository<User>, IUserRepository
    {
        public List<User> Users => Items;

        public User? GetByCuit(string cuit) => Users.FirstOrDefault(u => u.Cuit == cuit);
        public List<User> GetByBranch(Guid branchId) => Users.Where(u => u.BranchId == branchId && u.IsActive).ToList();
    }

    public class FakeBranchRepository : FakeRepository<Branch>, IBranchRepository
    {
        public List<Branch> Branches => Items;

        public Branch? GetById(Guid id) => Branches.FirstOrDefault(b => b.Id == id && b.IsActive);
        public List<Branch> GetByCompany(Guid companyId) => Branches.Where(b => b.CompanyId == companyId && b.IsActive).ToList();
    }

    public class FakeCompanyRepository : FakeRepository<Company>, ICompanyRepository
    {
        public List<Company> Companies => Items;

        public Company? GetById(Guid id) => Companies.FirstOrDefault(c => c.Id == id && c.IsActive);
        public List<Company> GetByOwner(string ownerCuit) =>
            Companies.Where(c => c.IsActive && c.Owners.Any(o => o.Cuit == ownerCuit)).ToList();
        public List<User> GetOwners(Guid companyId) => GetById(companyId)?.Owners.Where(o => o.IsActive).ToList() ?? new List<User>();
        public void AddOwner(Guid companyId, User owner) => GetById(companyId)?.Owners.Add(owner);
        public void RemoveOwner(Guid companyId, string ownerCuit) =>
            GetById(companyId)?.Owners.RemoveAll(o => o.Cuit == ownerCuit);
    }

    public class FakeProductRepository : FakeRepository<Product>, IProductRepository
    {
        public List<Product> Products => Items;
        public int UpdateRangeCalls { get; private set; }

        public List<Product> GetAll() => Products.Where(p => p.IsActive).ToList();
        public List<Product> GetAllIncludingInactive() => Products.ToList();
        public List<Product> GetByCompanies(List<Guid> companyIds) =>
            Products.Where(p => p.IsActive && companyIds.Contains(p.CompanyId)).ToList();
        public List<Product> GetByCompanyIncludingInactive(Guid companyId) => Products.Where(p => p.CompanyId == companyId).ToList();
        public Product? GetById(Guid id) => Products.FirstOrDefault(p => p.Id == id && p.IsActive);
        public void UpdateRange(List<Product> updatedProducts) => UpdateRangeCalls++;
        public void Delete(Product productToDelete, List<BranchStock> stocksToDelete)
        {
            productToDelete.IsActive = false;
            stocksToDelete.ForEach(s => s.IsActive = false);
        }
    }

    public class FakeBranchStockRepository : FakeRepository<BranchStock>, IBranchStockRepository
    {
        public List<BranchStock> Stocks => Items;

        public List<BranchStock> GetByBranch(Guid branchId) => Stocks.Where(s => s.BranchId == branchId && s.IsActive).ToList();
        public List<BranchStock> GetByBranchIncludingInactive(Guid branchId) => Stocks.Where(s => s.BranchId == branchId).ToList();
        public List<BranchStock> GetByProduct(Guid productId) => Stocks.Where(s => s.ProductId == productId && s.IsActive).ToList();
        public BranchStock? GetById(Guid branchId, Guid productId) =>
            Stocks.FirstOrDefault(s => s.BranchId == branchId && s.ProductId == productId);
    }

    public class FakeSaleRepository : ISaleRepository
    {
        public List<Sale> Sales { get; } = new();

        public Sale Create(Sale newSale, List<BranchStock> updatedStocks)
        {
            newSale.Number = Sales.Count + 1;
            Sales.Add(newSale);
            return newSale;
        }

        public List<Sale> GetByBranches(List<Guid> branchIds, DateTime from, DateTime to) =>
            Sales.Where(s => branchIds.Contains(s.BranchId) && s.Date >= from && s.Date < to).ToList();

        public List<Sale> GetByUser(string userCuit, DateTime from, DateTime to) =>
            Sales.Where(s => s.UserCuit == userCuit && s.Date >= from && s.Date < to).ToList();
    }

    public class FakePurchaseRepository : IPurchaseRepository
    {
        public List<Purchase> Purchases { get; } = new();

        public Purchase Create(Purchase newPurchase, List<BranchStock> updatedStocks)
        {
            newPurchase.Number = Purchases.Count + 1;
            Purchases.Add(newPurchase);
            return newPurchase;
        }
    }

    public class FakePasswordHashService : IPasswordHashService
    {
        public string Hash(User user, string password) => $"hash:{password}";
        public bool Verify(User user, string password) => user.PasswordHash == Hash(user, password);
    }

    public class FakeInflationService : IInflationService
    {
        public InflationSeries? Series { get; set; }

        public Task<InflationSeries?> GetSeriesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Series);
    }
}
