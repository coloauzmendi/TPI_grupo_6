using Application.Models;
using Application.Tests.Fakes;

namespace Application.Tests
{
    public class StockServiceTests
    {
        [Fact]
        public void GetItems_DevuelveSoloStockActivoOrdenadoPorNombre()
        {
            TestScenario scenario = new();
            scenario.StockOf(scenario.Hammer).IsActive = false;

            List<StockItemResponse> items = scenario.StockService.GetItems(scenario.Branch.Id);

            Assert.Equal(new[] { "Gaseosa", "Papas fritas" }, items.Select(i => i.Name));
            StockItemResponse cola = items[0];
            Assert.Equal(10, cola.Quantity);
            Assert.Equal(3, cola.MinimumStock);
            Assert.Equal(1000m, cola.Price);
        }
    }
}
