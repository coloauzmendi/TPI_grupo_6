using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Tests.Fakes
{
    // Escenario base: un dueño con una empresa, una sucursal y tres productos con stock
    public class TestScenario
    {
        public const string OwnerCuit = "20111111112";
        public const string OutsiderCuit = "20999999992";

        public FakeUserRepository Users { get; } = new();
        public FakeBranchRepository Branches { get; } = new();
        public FakeCompanyRepository Companies { get; } = new();
        public FakeProductRepository Products { get; } = new();
        public FakeBranchStockRepository Stocks { get; } = new();
        public FakeSaleRepository Sales { get; } = new();
        public FakePurchaseRepository Purchases { get; } = new();

        public Company Company { get; }
        public Branch Branch { get; }
        public Product Cola { get; }
        public Product Chips { get; }
        public Product Hammer { get; }

        public TestScenario()
        {
            User owner = Users.Create(new User { Cuit = OwnerCuit, FirstName = "Ana", LastName = "Perez", Role = UserRole.Owner });
            Users.Create(new User { Cuit = OutsiderCuit, FirstName = "Juan", LastName = "Gomez", Role = UserRole.Owner });

            Company = new Company { Id = Guid.NewGuid(), Name = "Kiosco Ana" };
            Company.Owners.Add(owner);
            Companies.Create(Company);

            Branch = Branches.Create(new Branch { Id = Guid.NewGuid(), CompanyId = Company.Id, Locality = "Rosario", Industry = Industry.Kiosk });

            Cola = AddProduct("Gaseosa", "Bebidas", Industry.Kiosk, 1000m, quantity: 10, minimum: 3);
            Chips = AddProduct("Papas fritas", "Snacks", Industry.Kiosk, 500m, quantity: 5, minimum: 2);
            Hammer = AddProduct("Martillo", "Herramientas", Industry.Hardware, 8000m, quantity: 2, minimum: 1);
        }

        public IAccessService Access => new AccessService(Users, Branches, Companies);

        public IPriceService PriceService => new PriceService(Products, Stocks, Access);
        public ISaleService SaleService => new SaleService(Sales, Stocks, Products, Access);
        public IPurchaseService PurchaseService => new PurchaseService(Purchases, Stocks, Products, Access);
        public IStockService StockService => new StockService(Stocks, Products, Access);
        public IBranchService BranchService => new BranchService(Branches, Users, Access);
        public ICompanyService CompanyService => new CompanyService(Companies, Users, Branches, BranchService, Access);
        public IProductService ProductService => new ProductService(Products, Stocks, Access);
        public IUserService UserService => new UserService(Users, Branches, new FakePasswordHashService(), Access);

        public BranchStock StockOf(Product product) => Stocks.GetById(Branch.Id, product.Id)!;

        // Otra empresa del dueño OutsiderCuit, con una sucursal y un producto igual a Cola (mismo nombre y marca)
        public Product AddOutsiderProduct()
        {
            Company company = new Company { Id = Guid.NewGuid(), Name = "Kiosco Juan" };
            company.Owners.Add(Users.GetByCuit(OutsiderCuit)!);
            Companies.Create(company);

            Branch branch = Branches.Create(new Branch { Id = Guid.NewGuid(), CompanyId = company.Id, Locality = "Funes", Industry = Industry.Kiosk });

            return AddProduct(branch, "Gaseosa", "Bebidas", Industry.Kiosk, 1000m, quantity: 7, minimum: 1);
        }

        private Product AddProduct(string name, string category, Industry industry, decimal price, int quantity, int minimum)
        {
            return AddProduct(Branch, name, category, industry, price, quantity, minimum);
        }

        private Product AddProduct(Branch branch, string name, string category, Industry industry, decimal price, int quantity, int minimum)
        {
            Product product = Products.Create(new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Brand = "Generica",
                Category = category,
                Industry = industry,
                Price = price,
                CompanyId = branch.CompanyId
            });
            Stocks.Create(new BranchStock { BranchId = branch.Id, ProductId = product.Id, Quantity = quantity, MinimumStock = minimum });
            return product;
        }
    }
}
