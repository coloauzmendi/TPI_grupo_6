using Application.Models;
using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class SaleServiceTests
    {
        [Fact]
        public void Create_VentaValida_DescuentaStockCalculaTotalYAvisaStockBajo()
        {
            TestScenario scenario = new();
            SaleRequest request = new()
            {
                Items =
                {
                    // El mismo producto repetido se suma: 2 + 1 = 3 papas
                    new SaleItemRequest { ProductId = scenario.Chips.Id, Quantity = 2 },
                    new SaleItemRequest { ProductId = scenario.Chips.Id, Quantity = 1 },
                    new SaleItemRequest { ProductId = scenario.Cola.Id, Quantity = 1 }
                }
            };

            SaleResultResponse result = scenario.SaleService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, request);

            Assert.Equal(2, scenario.StockOf(scenario.Chips).Quantity);
            Assert.Equal(9, scenario.StockOf(scenario.Cola).Quantity);
            Assert.Equal(3 * 500m + 1000m, result.Sale.Total);
            Assert.Equal("V-000001", result.Sale.Code);
            Assert.Single(scenario.Sales.Sales);
            Assert.Single(result.Alerts);
            Assert.Contains("Papas fritas", result.Alerts[0]);
        }

        [Fact]
        public void Create_StockInsuficiente_LanzaValidacionYNoGuarda()
        {
            TestScenario scenario = new();
            SaleRequest request = new()
            {
                Items = { new SaleItemRequest { ProductId = scenario.Hammer.Id, Quantity = 5 } }
            };

            Assert.Throws<AppValidationException>(() =>
                scenario.SaleService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, request));
            Assert.Equal(2, scenario.StockOf(scenario.Hammer).Quantity);
            Assert.Empty(scenario.Sales.Sales);
        }

        [Fact]
        public void Create_LineaNegativaDelMismoProducto_LanzaValidacionYNoGuarda()
        {
            TestScenario scenario = new();
            SaleRequest request = new()
            {
                Items =
                {
                    new SaleItemRequest { ProductId = scenario.Cola.Id, Quantity = 10 },
                    new SaleItemRequest { ProductId = scenario.Cola.Id, Quantity = -5 }
                }
            };

            Assert.Throws<AppValidationException>(() =>
                scenario.SaleService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, request));
            Assert.Equal(10, scenario.StockOf(scenario.Cola).Quantity);
            Assert.Empty(scenario.Sales.Sales);
        }

        [Fact]
        public void Create_SucursalAjena_LanzaNotAllowed()
        {
            TestScenario scenario = new();
            SaleRequest request = new()
            {
                Items = { new SaleItemRequest { ProductId = scenario.Cola.Id, Quantity = 1 } }
            };

            Assert.Throws<NotAllowedException>(() =>
                scenario.SaleService.Create(TestScenario.OutsiderCuit, scenario.Branch.Id, request));
            Assert.Equal(10, scenario.StockOf(scenario.Cola).Quantity);
        }

        [Fact]
        public void Create_DuenoConAccesoALaSucursal_RegistraLaVenta()
        {
            TestScenario scenario = new();
            SaleRequest request = new()
            {
                Items = { new SaleItemRequest { ProductId = scenario.Cola.Id, Quantity = 2 } }
            };

            SaleResultResponse result = scenario.SaleService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, request);

            Assert.Equal(8, scenario.StockOf(scenario.Cola).Quantity);
            Assert.Equal(TestScenario.OwnerCuit, scenario.Sales.Sales.Single().UserCuit);
            Assert.Equal(2000m, result.Sale.Total);
        }

        [Fact]
        public void Create_DuenoSinAccesoALaSucursal_LanzaNotAllowedYNoGuarda()
        {
            TestScenario scenario = new();
            SaleRequest request = new()
            {
                Items = { new SaleItemRequest { ProductId = scenario.Cola.Id, Quantity = 1 } }
            };

            Assert.Throws<NotAllowedException>(() =>
                scenario.SaleService.Create(TestScenario.OutsiderCuit, scenario.Branch.Id, request));
            Assert.Equal(10, scenario.StockOf(scenario.Cola).Quantity);
            Assert.Empty(scenario.Sales.Sales);
        }

        [Fact]
        public void Cancel_VentaValida_DevuelveElStockYLaExcluyeDeLosReportes()
        {
            TestScenario scenario = new();
            SaleResponse sale = CreateSale(scenario, TestScenario.OwnerCuit, scenario.Cola, 4);

            scenario.SaleService.Cancel(TestScenario.OwnerCuit, scenario.Branch.Id, sale.Id);

            Assert.Equal(10, scenario.StockOf(scenario.Cola).Quantity);
            Assert.True(scenario.Sales.Sales.Single().IsCancelled);
            Assert.Empty(scenario.Sales.GetByBranches(new List<Guid> { scenario.Branch.Id }, DateTime.Today, DateTime.Today.AddDays(1)));
        }

        [Fact]
        public void Cancel_StockDadoDeBaja_DevuelveElStockIgual()
        {
            TestScenario scenario = new();
            SaleResponse sale = CreateSale(scenario, TestScenario.OwnerCuit, scenario.Cola, 4);
            scenario.StockOf(scenario.Cola).IsActive = false;

            scenario.SaleService.Cancel(TestScenario.OwnerCuit, scenario.Branch.Id, sale.Id);

            Assert.Equal(10, scenario.StockOf(scenario.Cola).Quantity);
            Assert.False(scenario.StockOf(scenario.Cola).IsActive);
        }

        [Fact]
        public void Cancel_EmpleadoConVentaAjena_LanzaNotAllowedYNoDevuelveStock()
        {
            TestScenario scenario = new();
            AddEmployee(scenario, EmployeeCuit);
            AddEmployee(scenario, OtherEmployeeCuit);
            SaleResponse sale = CreateSale(scenario, OtherEmployeeCuit, scenario.Cola, 4);

            Assert.Throws<NotAllowedException>(() =>
                scenario.SaleService.Cancel(EmployeeCuit, scenario.Branch.Id, sale.Id));
            Assert.Equal(6, scenario.StockOf(scenario.Cola).Quantity);
            Assert.False(scenario.Sales.Sales.Single().IsCancelled);
        }

        [Fact]
        public void Cancel_EmpleadoConVentaPropia_LaAnula()
        {
            TestScenario scenario = new();
            AddEmployee(scenario, EmployeeCuit);
            SaleResponse sale = CreateSale(scenario, EmployeeCuit, scenario.Cola, 4);

            scenario.SaleService.Cancel(EmployeeCuit, scenario.Branch.Id, sale.Id);

            Assert.Equal(10, scenario.StockOf(scenario.Cola).Quantity);
        }

        [Fact]
        public void Cancel_VentaYaAnulada_LanzaValidacionYNoDevuelveDosVeces()
        {
            TestScenario scenario = new();
            SaleResponse sale = CreateSale(scenario, TestScenario.OwnerCuit, scenario.Cola, 4);
            scenario.SaleService.Cancel(TestScenario.OwnerCuit, scenario.Branch.Id, sale.Id);

            Assert.Throws<AppValidationException>(() =>
                scenario.SaleService.Cancel(TestScenario.OwnerCuit, scenario.Branch.Id, sale.Id));
            Assert.Equal(10, scenario.StockOf(scenario.Cola).Quantity);
        }

        [Fact]
        public void Cancel_VentaInexistente_LanzaNotFound()
        {
            TestScenario scenario = new();

            Assert.Throws<NotFoundException>(() =>
                scenario.SaleService.Cancel(TestScenario.OwnerCuit, scenario.Branch.Id, Guid.NewGuid()));
        }

        [Fact]
        public void Cancel_NumeroDeComprobanteNoSeReutiliza()
        {
            TestScenario scenario = new();
            SaleResponse first = CreateSale(scenario, TestScenario.OwnerCuit, scenario.Cola, 1);
            scenario.SaleService.Cancel(TestScenario.OwnerCuit, scenario.Branch.Id, first.Id);

            SaleResponse second = CreateSale(scenario, TestScenario.OwnerCuit, scenario.Cola, 1);

            Assert.Equal("V-000002", second.Code);
        }

        private const string EmployeeCuit = "20333333332";
        private const string OtherEmployeeCuit = "20444444442";

        private static void AddEmployee(TestScenario scenario, string cuit)
        {
            scenario.Users.Create(new User { Cuit = cuit, FirstName = "Empleado", LastName = cuit, Role = UserRole.Employee, BranchId = scenario.Branch.Id });
        }

        private static SaleResponse CreateSale(TestScenario scenario, string cuit, Product product, int quantity)
        {
            SaleRequest request = new()
            {
                Items = { new SaleItemRequest { ProductId = product.Id, Quantity = quantity } }
            };

            return scenario.SaleService.Create(cuit, scenario.Branch.Id, request).Sale;
        }
    }
}