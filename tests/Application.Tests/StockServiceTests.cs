using Application.Models;
using Application.Services;
using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class StockServiceTests
    {
        [Fact]
        public void GetItems_DevuelveSoloStockActivoOrdenadoPorNombre()
        {
            TestScenario scenario = new();
            scenario.StockOf(scenario.Hammer).IsActive = false;

            List<StockItemResponse> items = scenario.StockService.GetItems(TestScenario.OwnerCuit, scenario.Branch.Id);

            Assert.Equal(new[] { "Gaseosa", "Papas fritas" }, items.Select(i => i.Name));
            StockItemResponse cola = items[0];
            Assert.Equal(10, cola.Quantity);
            Assert.Equal(3, cola.MinimumStock);
            Assert.Equal(1000m, cola.Price);
        }

        [Fact]
        public void GetItems_IncludeInactive_DevuelveStockDadoDeBajaMarcadoComoInactivo()
        {
            TestScenario scenario = new();
            scenario.StockOf(scenario.Hammer).IsActive = false;

            List<StockItemResponse> items = scenario.StockService.GetItems(TestScenario.OwnerCuit, scenario.Branch.Id, includeInactive: true);

            Assert.Equal(new[] { "Gaseosa", "Martillo", "Papas fritas" }, items.Select(i => i.Name));
            Assert.False(items.Single(i => i.Name == "Martillo").IsActive);
            Assert.All(items.Where(i => i.Name != "Martillo"), i => Assert.True(i.IsActive));
        }

        [Fact]
        public void GetItems_IncludeInactive_NoDevuelveProductosDadosDeBaja()
        {
            TestScenario scenario = new();
            scenario.ProductService.Delete(TestScenario.OwnerCuit, scenario.Chips.Id);

            List<StockItemResponse> items = scenario.StockService.GetItems(TestScenario.OwnerCuit, scenario.Branch.Id, includeInactive: true);

            Assert.Equal(new[] { "Gaseosa", "Martillo" }, items.Select(i => i.Name));
        }

        [Fact]
        public void GetItems_IncludeInactiveComoEmpleado_SeIgnora()
        {
            TestScenario scenario = new();
            scenario.Users.Create(new User { Cuit = "20-33333333-3", Role = UserRole.Employee, BranchId = scenario.Branch.Id });
            scenario.StockOf(scenario.Hammer).IsActive = false;

            List<StockItemResponse> items = scenario.StockService.GetItems("20-33333333-3", scenario.Branch.Id, includeInactive: true);

            Assert.Equal(new[] { "Gaseosa", "Papas fritas" }, items.Select(i => i.Name));
        }

        [Fact]
        public void Create_ProductoYaCargado_LanzaConflicto()
        {
            TestScenario scenario = new();
            BranchStockCreateRequest stock = new() { ProductId = scenario.Cola.Id, Quantity = 1 };

            Assert.Throws<ConflictException>(() => scenario.StockService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, stock));
        }

        [Fact]
        public void Create_StockDadoDeBaja_LoReactivaConLosDatosNuevos()
        {
            TestScenario scenario = new();
            scenario.StockService.Delete(TestScenario.OwnerCuit, scenario.Branch.Id, scenario.Hammer.Id);
            BranchStockCreateRequest stock = new() { ProductId = scenario.Hammer.Id, Quantity = 7, MinimumStock = 2, UnitCost = 5000m };

            BranchStockResponse result = scenario.StockService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, stock);

            BranchStock saved = scenario.StockOf(scenario.Hammer);
            Assert.True(saved.IsActive);
            Assert.Equal(7, saved.Quantity);
            Assert.Equal(2, saved.MinimumStock);
            Assert.Equal(scenario.Hammer.Id, result.ProductId);
            Assert.Equal(7, result.Quantity);
        }

        [Fact]
        public void Create_ProductoInexistente_LanzaValidacion()
        {
            TestScenario scenario = new();
            BranchStockCreateRequest stock = new() { ProductId = Guid.NewGuid(), Quantity = 1 };

            Assert.Throws<AppValidationException>(() => scenario.StockService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, stock));
        }

        [Fact]
        public void Update_CambiaCantidadYMinimo()
        {
            TestScenario scenario = new();
            BranchStockUpdateRequest stock = new() { Quantity = 0, MinimumStock = 4 };

            scenario.StockService.Update(TestScenario.OwnerCuit, scenario.Branch.Id, scenario.Cola.Id, stock);

            Assert.Equal(0, scenario.StockOf(scenario.Cola).Quantity);
            Assert.Equal(4, scenario.StockOf(scenario.Cola).MinimumStock);
        }

        [Fact]
        public void GetById_StockDadoDeBaja_LanzaNotFound()
        {
            TestScenario scenario = new();
            scenario.StockService.Delete(TestScenario.OwnerCuit, scenario.Branch.Id, scenario.Cola.Id);

            Assert.Throws<NotFoundException>(() => scenario.StockService.GetById(TestScenario.OwnerCuit, scenario.Branch.Id, scenario.Cola.Id));
        }

        [Fact]
        public void GetItems_SucursalAjena_LanzaNotAllowed()
        {
            TestScenario scenario = new();

            Assert.Throws<NotAllowedException>(() => scenario.StockService.GetItems(TestScenario.OutsiderCuit, scenario.Branch.Id));
        }

        [Fact]
        public void Create_ProductoDeOtraEmpresa_LanzaValidacionYNoCargaStock()
        {
            TestScenario scenario = new();
            Product outsider = scenario.AddOutsiderProduct();

            Assert.Throws<AppValidationException>(() => scenario.StockService.Create(TestScenario.OwnerCuit, scenario.Branch.Id,
                new BranchStockCreateRequest { ProductId = outsider.Id, Quantity = 5, MinimumStock = 1 }));
            Assert.Null(scenario.Stocks.GetById(scenario.Branch.Id, outsider.Id));
        }

        [Fact]
        public void GetItems_ProductoDadoDeBaja_NoAparece()
        {
            TestScenario scenario = new();
            scenario.ProductService.Delete(TestScenario.OwnerCuit, scenario.Chips.Id);

            List<StockItemResponse> items = scenario.StockService.GetItems(TestScenario.OwnerCuit, scenario.Branch.Id);

            Assert.Equal(new[] { "Gaseosa", "Martillo" }, items.Select(i => i.Name));
        }

        [Fact]
        public void Create_ConCostoTotal_DaDeAltaElStockYRegistraLaCompraConElCostoUnitario()
        {
            TestScenario scenario = new();
            Product water = AddProductWithoutStock(scenario);
            BranchStockCreateRequest stock = new() { ProductId = water.Id, Quantity = 3, MinimumStock = 1, TotalCost = 1000m };

            scenario.StockService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, stock);

            Assert.Equal(3, scenario.StockOf(water).Quantity);
            Purchase purchase = scenario.Purchases.Purchases.Single();
            Assert.Equal(StockService.InitialLoadSupplier, purchase.Supplier);
            Assert.Equal(scenario.Branch.Id, purchase.BranchId);
            Assert.Equal(TestScenario.OwnerCuit, purchase.UserCuit);
            PurchaseDetail detail = purchase.Details.Single();
            Assert.Equal(water.Id, detail.ProductId);
            Assert.Equal(3, detail.Quantity);
            Assert.Equal(333.33m, detail.UnitCost);
            Assert.Equal(999.99m, purchase.Total);
        }

        [Fact]
        public void Create_Reactivacion_RegistraLaCompraConElCostoUnitario()
        {
            TestScenario scenario = new();
            scenario.StockService.Delete(TestScenario.OwnerCuit, scenario.Branch.Id, scenario.Hammer.Id);
            BranchStockCreateRequest stock = new() { ProductId = scenario.Hammer.Id, Quantity = 4, MinimumStock = 1, UnitCost = 5000m };

            scenario.StockService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, stock);

            PurchaseDetail detail = scenario.Purchases.Purchases.Single().Details.Single();
            Assert.Equal(5000m, detail.UnitCost);
            Assert.Equal(4, detail.Quantity);
            Assert.Equal(20000m, scenario.Purchases.Purchases.Single().Total);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Create_SinCostoOConAmbos_LanzaValidacionYNoCargaNada(bool withUnitCost, bool withTotalCost)
        {
            TestScenario scenario = new();
            Product water = AddProductWithoutStock(scenario);
            BranchStockCreateRequest stock = new()
            {
                ProductId = water.Id,
                Quantity = 3,
                UnitCost = withUnitCost ? 100m : null,
                TotalCost = withTotalCost ? 300m : null
            };

            Assert.Throws<AppValidationException>(() => scenario.StockService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, stock));
            Assert.Null(scenario.Stocks.GetById(scenario.Branch.Id, water.Id));
            Assert.Empty(scenario.Purchases.Purchases);
        }

        private static Product AddProductWithoutStock(TestScenario scenario)
        {
            return scenario.Products.Create(new Product
            {
                Id = Guid.NewGuid(),
                Name = "Agua",
                Brand = "Generica",
                Category = "Bebidas",
                Industry = Industry.Kiosk,
                Price = 700m,
                CompanyId = scenario.Company.Id
            });
        }
    }
}
