using Application.Models;
using Application.Tests.Fakes;
using Domain.Exceptions;

namespace Application.Tests
{
    public class PurchaseServiceTests
    {
        [Fact]
        public void Create_CompraValida_IncrementaStockYCalculaTotal()
        {
            TestScenario scenario = new();
            PurchaseRequest request = new()
            {
                Supplier = "  Distribuidora Norte  ",
                Items =
                {
                    new PurchaseItemRequest { ProductId = scenario.Cola.Id, Quantity = 12, UnitCost = 600m },
                    new PurchaseItemRequest { ProductId = scenario.Hammer.Id, Quantity = 1, UnitCost = 5000m }
                }
            };

            PurchaseResponse result = scenario.PurchaseService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, request);

            Assert.Equal(22, scenario.StockOf(scenario.Cola).Quantity);
            Assert.Equal(3, scenario.StockOf(scenario.Hammer).Quantity);
            Assert.Equal(12 * 600m + 5000m, result.Total);
            Assert.Equal("Distribuidora Norte", result.Supplier);
            Assert.Equal("C-00001", result.Code);
            Assert.Equal(22, result.Items.Single(i => i.ProductId == scenario.Cola.Id).NewStock);
        }

        [Fact]
        public void Create_CostoNegativo_LanzaValidacionYNoModificaStock()
        {
            TestScenario scenario = new();
            PurchaseRequest request = new()
            {
                Items = { new PurchaseItemRequest { ProductId = scenario.Cola.Id, Quantity = 5, UnitCost = -1m } }
            };

            Assert.Throws<AppValidationException>(() =>
                scenario.PurchaseService.Create(TestScenario.OwnerCuit, scenario.Branch.Id, request));
            Assert.Equal(10, scenario.StockOf(scenario.Cola).Quantity);
            Assert.Empty(scenario.Purchases.Purchases);
        }
    }
}
