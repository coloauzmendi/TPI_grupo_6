using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Infrastructure.Data
{
    public class SeedDataBuilder
    {
        private readonly IPasswordHashService _passwordHashService;
        private int _lastSaleNumber;
        private int _lastPurchaseNumber;

        public SeedDataBuilder(IPasswordHashService passwordHashService)
        {
            _passwordHashService = passwordHashService;
        }

        public List<User> Users { get; } = new();
        public List<Company> Companies { get; } = new();
        public List<Branch> Branches { get; } = new();
        public List<Product> Products { get; } = new();
        public List<BranchStock> Stocks { get; } = new();
        public List<Purchase> Purchases { get; } = new();
        public List<Sale> Sales { get; } = new();

        public User AddOwner(string cuit, string firstName, string lastName, string password)
        {
            return AddUser(cuit, firstName, lastName, UserRole.Owner, null, password);
        }

        public User AddEmployee(string cuit, string firstName, string lastName, Branch branch, string password)
        {
            return AddUser(cuit, firstName, lastName, UserRole.Employee, branch.Id, password);
        }

        public Company AddCompany(string name, params User[] owners)
        {
            Company company = new Company { Id = Guid.NewGuid(), Name = name };
            company.Owners.AddRange(owners);
            Companies.Add(company);

            return company;
        }

        public Branch AddBranch(Company company, string locality, string address, Industry industry)
        {
            Branch branch = new Branch
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                Locality = locality,
                Address = address,
                Industry = industry
            };
            Branches.Add(branch);

            return branch;
        }

        public Product AddProduct(Company company, string name, string brand, string category, decimal price, Industry industry)
        {
            Product product = new Product
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                Name = name,
                Brand = brand,
                Category = category,
                Price = price,
                Industry = industry
            };
            Products.Add(product);

            return product;
        }

        public BranchStock AddInitialLoad(Branch branch, User user, Product product, int quantity, int minimumStock, decimal unitCost, DateTime date)
        {
            EnsureCanOperate(user, branch);
            if (product.CompanyId != branch.CompanyId)
            {
                throw new AppValidationException("El producto no pertenece a la empresa de la sucursal.");
            }

            if (FindStock(branch, product) is not null)
            {
                throw new ConflictException("Ese producto ya tiene stock cargado en esta sucursal.");
            }

            StockService.EnsureMaxStock(quantity);
            BranchStock stock = new BranchStock { BranchId = branch.Id, ProductId = product.Id, MinimumStock = minimumStock };
            stock.SetQuantity(quantity);
            Stocks.Add(stock);

            Purchase purchase = NewPurchase(branch, user, StockService.InitialLoadSupplier, date);
            AddPurchaseDetail(purchase, product, quantity, unitCost);
            CompletePurchase(purchase);

            return stock;
        }

        public Purchase AddPurchase(Branch branch, User user, string supplier, DateTime date, params (Product Product, int Quantity, decimal UnitCost)[] items)
        {
            EnsureCanOperate(user, branch);

            Purchase purchase = NewPurchase(branch, user, supplier, date);
            foreach (var item in items)
            {
                BranchStock stock = GetActiveStock(branch, item.Product);
                StockService.EnsureMaxStock((long)stock.Quantity + item.Quantity);
                stock.Add(item.Quantity);
                AddPurchaseDetail(purchase, item.Product, item.Quantity, item.UnitCost);
            }

            CompletePurchase(purchase);

            return purchase;
        }

        public Sale AddSale(Branch branch, User user, DateTime date, params (Product Product, int Quantity)[] items)
        {
            EnsureCanOperate(user, branch);
            if (items.Length == 0 || items.Select(i => i.Product.Id).Distinct().Count() != items.Length)
            {
                throw new AppValidationException("La venta debe tener productos distintos.");
            }

            Sale sale = new Sale { Id = Guid.NewGuid(), BranchId = branch.Id, UserCuit = user.Cuit, Date = date };
            foreach (var item in items)
            {
                if (!item.Product.IsActive)
                {
                    throw new AppValidationException("El producto no está disponible en esta sucursal.");
                }

                GetActiveStock(branch, item.Product).Remove(item.Quantity);
                sale.Details.Add(new SaleDetail
                {
                    SaleId = sale.Id,
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price
                });
            }

            sale.Total = sale.Details.Sum(d => d.Quantity * d.UnitPrice);
            sale.Number = ++_lastSaleNumber;
            Sales.Add(sale);

            return sale;
        }

        public void CancelSale(Sale sale)
        {
            if (sale.IsCancelled)
            {
                throw new AppValidationException("La venta ya está anulada.");
            }

            foreach (SaleDetail detail in sale.Details)
            {
                Stocks.First(s => s.BranchId == sale.BranchId && s.ProductId == detail.ProductId).Add(detail.Quantity);
            }

            sale.IsCancelled = true;
        }

        public void DeactivateProduct(Product product)
        {
            product.IsActive = false;
            foreach (BranchStock stock in Stocks.Where(s => s.ProductId == product.Id))
            {
                stock.IsActive = false;
            }
        }

        public BranchStock? FindStock(Branch branch, Product product)
        {
            return Stocks.FirstOrDefault(s => s.BranchId == branch.Id && s.ProductId == product.Id);
        }

        private User AddUser(string cuit, string firstName, string lastName, UserRole role, Guid? branchId, string password)
        {
            User user = new User
            {
                Cuit = cuit,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                BranchId = branchId
            };
            user.PasswordHash = _passwordHashService.Hash(user, password);
            Users.Add(user);

            return user;
        }

        private void EnsureCanOperate(User user, Branch branch)
        {
            bool allowed = user.Role == UserRole.Employee
                ? user.BranchId == branch.Id
                : Companies.Any(c => c.Id == branch.CompanyId && c.Owners.Contains(user));

            if (!allowed)
            {
                throw new NotAllowedException("No tenés acceso a esta sucursal.");
            }
        }

        private BranchStock GetActiveStock(Branch branch, Product product)
        {
            BranchStock? stock = FindStock(branch, product);
            if (stock is null || !stock.IsActive)
            {
                throw new AppValidationException("El producto no está dado de alta en esta sucursal.");
            }

            return stock;
        }

        private static Purchase NewPurchase(Branch branch, User user, string supplier, DateTime date)
        {
            return new Purchase { Id = Guid.NewGuid(), BranchId = branch.Id, UserCuit = user.Cuit, Supplier = supplier, Date = date };
        }

        private static void AddPurchaseDetail(Purchase purchase, Product product, int quantity, decimal unitCost)
        {
            purchase.Details.Add(new PurchaseDetail
            {
                PurchaseId = purchase.Id,
                ProductId = product.Id,
                Quantity = quantity,
                UnitCost = UnitCostCalculator.Resolve(unitCost, null, quantity)
            });
        }

        private void CompletePurchase(Purchase purchase)
        {
            purchase.Total = purchase.Details.Sum(d => d.Quantity * d.UnitCost);
            purchase.Number = ++_lastPurchaseNumber;
            Purchases.Add(purchase);
        }
    }
}
