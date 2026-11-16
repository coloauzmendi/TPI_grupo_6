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

        public Branch Branch { get; }
        public Product Cola { get; }
        public Product Chips { get; }
        public Product Hammer { get; }

        public TestScenario()
        {
            User owner = Users.Create(new User { Cuit = OwnerCuit, FirstName = "Ana", LastName = "Perez", Role = UserRole.Owner });
            Users.Create(new User { Cuit = OutsiderCuit, FirstName = "Juan", LastName = "Gomez", Role = UserRole.Owner });

            Company company = new Company { Id = Guid.NewGuid(), Name = "Kiosco Ana" };
            company.Owners.Add(owner);
            Companies.Create(company);

            Branch = Branches.Create(new Branch { Id = Guid.NewGuid(), CompanyId = company.Id, Locality = "Rosario", Industry = Industry.Kiosk });

            Cola = AddProduct("Gaseosa", "Bebidas", Industry.Kiosk, 1000m, quantity: 10, minimum: 3);
            Chips = AddProduct("Papas fritas", "Snacks", Industry.Kiosk, 500m, quantity: 5, minimum: 2);
            Hammer = AddProduct("Martillo", "Herramientas", Industry.Hardware, 8000m, quantity: 2, minimum: 1);
        }

        public AccessService Access => new AccessService(Users, Branches, Companies);

        public PriceService PriceService => new PriceService(Products, Stocks, Access);
        public SaleService SaleService => new SaleService(Sales, Stocks, Products, Access);
        public PurchaseService PurchaseService => new PurchaseService(Purchases, Stocks, Products, Access);
        public StockService StockService => new StockService(Stocks, Products);

        public BranchStock StockOf(Product product) => Stocks.GetById(Branch.Id, product.Id)!;

        private Product AddProduct(string name, string category, Industry industry, decimal price, int quantity, int minimum)
        {
            Product product = Products.Create(new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Brand = "Generica",
                Category = category,
                Industry = industry,
                Price = price
            });
            Stocks.Create(new BranchStock { BranchId = Branch.Id, ProductId = product.Id, Quantity = quantity, MinimumStock = minimum });
            return product;
        }
    }
}
