using Application.Models;
using Application.Tests.Fakes;
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
    }
}
