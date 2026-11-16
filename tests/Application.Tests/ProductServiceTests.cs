using Application.Models;
using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class ProductServiceTests
    {
        private const string EmployeeCuit = "20-33333333-3";

        [Fact]
        public void Delete_DaDeBajaElProductoYSuStockActivo()
        {
            TestScenario scenario = new();
            Branch other = scenario.Branches.Create(new Branch { Id = Guid.NewGuid(), CompanyId = scenario.Company.Id, Locality = "Funes" });
            BranchStock otherStock = scenario.Stocks.Create(new BranchStock { BranchId = other.Id, ProductId = scenario.Cola.Id });
            otherStock.SetQuantity(4);

            scenario.ProductService.Delete(TestScenario.OwnerCuit, scenario.Cola.Id);

            Assert.False(scenario.Cola.IsActive);
            Assert.False(scenario.StockOf(scenario.Cola).IsActive);
            Assert.False(otherStock.IsActive);
            Assert.True(scenario.StockOf(scenario.Chips).IsActive);
        }

        [Fact]
        public void Delete_ProductoInexistente_LanzaNotFound()
        {
            TestScenario scenario = new();

            Assert.Throws<NotFoundException>(() => scenario.ProductService.Delete(TestScenario.OwnerCuit, Guid.NewGuid()));
        }

        [Fact]
        public void Create_MismoNombreYMarcaActivo_LanzaConflictoYNoCrea()
        {
            TestScenario scenario = new();
            ProductCreateRequest duplicate = new() { Name = "  GASEOSA ", Brand = "generica", Category = "Bebidas", Price = 1200m, CompanyId = scenario.Company.Id };

            Assert.Throws<ConflictException>(() => scenario.ProductService.Create(TestScenario.OwnerCuit, duplicate));
            Assert.Equal(3, scenario.Products.Products.Count);
        }

        [Fact]
        public void Create_MismoNombreYMarcaInactivo_LoReactivaConLosDatosNuevos()
        {
            TestScenario scenario = new();
            scenario.ProductService.Delete(TestScenario.OwnerCuit, scenario.Hammer.Id);
            ProductCreateRequest again = new() { Name = "martillo", Brand = " Generica", Category = "Ferretería", Price = 9500m, Industry = Industry.Kiosk, CompanyId = scenario.Company.Id };

            ProductResponse result = scenario.ProductService.Create(TestScenario.OwnerCuit, again);

            Assert.Equal(scenario.Hammer.Id, result.Id);
            Assert.True(scenario.Hammer.IsActive);
            Assert.Equal("Martillo", result.Name);
            Assert.Equal("Ferretería", result.Category);
            Assert.Equal(9500m, result.Price);
            Assert.Equal(Industry.Kiosk, result.Industry);
            Assert.Equal(3, scenario.Products.Products.Count);
        }

        [Fact]
        public void Create_ProductoNuevo_LoAgregaEnLaEmpresa()
        {
            TestScenario scenario = new();
            ProductCreateRequest product = new() { Name = "Gaseosa", Brand = "Otra marca", Category = "Bebidas", Price = 900m, CompanyId = scenario.Company.Id };

            ProductResponse result = scenario.ProductService.Create(TestScenario.OwnerCuit, product);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Otra marca", result.Brand);
            Assert.Equal(scenario.Company.Id, result.CompanyId);
            Assert.Equal(4, scenario.Products.Products.Count);
        }

        [Fact]
        public void Create_MismoNombreYMarcaQueEnOtraEmpresa_LoCrea()
        {
            TestScenario scenario = new();
            Company second = new Company { Id = Guid.NewGuid(), Name = "Kiosco Ana 2" };
            second.Owners.Add(scenario.Users.GetByCuit(TestScenario.OwnerCuit)!);
            scenario.Companies.Create(second);
            ProductCreateRequest product = new() { Name = "Gaseosa", Brand = "Generica", Category = "Bebidas", Price = 900m, CompanyId = second.Id };

            ProductResponse result = scenario.ProductService.Create(TestScenario.OwnerCuit, product);

            Assert.NotEqual(scenario.Cola.Id, result.Id);
            Assert.Equal(second.Id, result.CompanyId);
            Assert.Equal(1000m, scenario.Cola.Price);
            Assert.Equal(4, scenario.Products.Products.Count);
        }

        [Fact]
        public void Create_EnEmpresaAjena_LanzaNotAllowedYNoCrea()
        {
            TestScenario scenario = new();
            ProductCreateRequest product = new() { Name = "Alfajor", Brand = "Generica", Category = "Golosinas", Price = 700m, CompanyId = scenario.Company.Id };

            Assert.Throws<NotAllowedException>(() => scenario.ProductService.Create(TestScenario.OutsiderCuit, product));
            Assert.Equal(3, scenario.Products.Products.Count);
        }

        [Fact]
        public void GetAll_Dueno_VeSoloLosProductosDeSusEmpresas()
        {
            TestScenario scenario = new();
            Product outsider = scenario.AddOutsiderProduct();

            List<ProductResponse> own = scenario.ProductService.GetAll(TestScenario.OwnerCuit);
            List<ProductResponse> others = scenario.ProductService.GetAll(TestScenario.OutsiderCuit);

            Assert.Equal(3, own.Count);
            Assert.DoesNotContain(own, p => p.Id == outsider.Id);
            Assert.Equal(new[] { outsider.Id }, others.Select(p => p.Id));
        }

        [Fact]
        public void GetAll_Empleado_VeLosProductosDeLaEmpresaDeSuSucursal()
        {
            TestScenario scenario = new();
            scenario.AddOutsiderProduct();
            scenario.Users.Create(new User { Cuit = EmployeeCuit, Role = UserRole.Employee, BranchId = scenario.Branch.Id });

            List<ProductResponse> products = scenario.ProductService.GetAll(EmployeeCuit);

            Assert.Equal(3, products.Count);
            Assert.All(products, p => Assert.Equal(scenario.Company.Id, p.CompanyId));
        }

        [Fact]
        public void GetAll_FiltrandoPorEmpresaAjena_LanzaNotAllowed()
        {
            TestScenario scenario = new();
            Product outsider = scenario.AddOutsiderProduct();

            Assert.Throws<NotAllowedException>(() => scenario.ProductService.GetAll(TestScenario.OwnerCuit, outsider.CompanyId));
        }

        [Fact]
        public void GetById_ProductoDeOtraEmpresa_LanzaNotFound()
        {
            TestScenario scenario = new();
            Product outsider = scenario.AddOutsiderProduct();

            Assert.Throws<NotFoundException>(() => scenario.ProductService.GetById(TestScenario.OwnerCuit, outsider.Id));
        }

        [Fact]
        public void Update_ProductoDeOtraEmpresa_LanzaNotAllowedYNoCambia()
        {
            TestScenario scenario = new();
            ProductRequest change = new() { Name = "Gaseosa", Brand = "Generica", Category = "Bebidas", Price = 1m };

            Assert.Throws<NotAllowedException>(() => scenario.ProductService.Update(TestScenario.OutsiderCuit, scenario.Cola.Id, change));
            Assert.Equal(1000m, scenario.Cola.Price);
        }

        [Fact]
        public void Delete_ProductoDeOtraEmpresa_LanzaNotAllowedYNoLoDaDeBaja()
        {
            TestScenario scenario = new();

            Assert.Throws<NotAllowedException>(() => scenario.ProductService.Delete(TestScenario.OutsiderCuit, scenario.Cola.Id));
            Assert.True(scenario.Cola.IsActive);
        }
    }
}
