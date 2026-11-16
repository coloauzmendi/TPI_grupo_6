using Application.Models;
using Application.Tests.Fakes;
using Domain.Entities;

namespace Application.Tests
{
    public class ReportServiceTests
    {
        private static readonly DateTime Month = new DateTime(2026, 10, 1);

        [Fact]
        public async Task GetMonthlyClose_UsaElCostoDeLaCompraMasRecienteYExcluyeProductosSinCompras()
        {
            TestScenario scenario = new();
            AddPurchase(scenario, Month.AddMonths(-2), 1, scenario.Cola, 400m);
            AddPurchase(scenario, Month.AddDays(-5), 2, scenario.Cola, 600m);
            AddPurchase(scenario, Month.AddDays(3), 3, scenario.Chips, 200m);
            AddSale(scenario, Month.AddDays(10), (scenario.Cola, 3, 1000m), (scenario.Chips, 2, 500m), (scenario.Hammer, 1, 8000m));

            MonthlyCloseResponse result = await scenario.ReportService.GetMonthlyCloseAsync(TestScenario.OwnerCuit, null, null, Month.Year, Month.Month);

            Assert.Equal(3 * (1000m - 600m) + 2 * (500m - 200m), result.EstimatedProfit);
            Assert.Equal(1, result.ProductsWithoutCost);
        }

        [Fact]
        public async Task GetMonthlyClose_SoloConsideraLasVentasDelMesConsultado()
        {
            TestScenario scenario = new();
            AddPurchase(scenario, Month.AddMonths(-3), 1, scenario.Cola, 600m);
            AddSale(scenario, Month.AddMonths(-1), (scenario.Cola, 5, 1000m));
            AddSale(scenario, Month.AddDays(1), (scenario.Cola, 1, 1000m));

            MonthlyCloseResponse result = await scenario.ReportService.GetMonthlyCloseAsync(TestScenario.OwnerCuit, null, null, Month.Year, Month.Month);

            Assert.Equal(400m, result.EstimatedProfit);
            Assert.Equal(0, result.ProductsWithoutCost);
        }

        [Fact]
        public async Task GetMonthlyClose_VentaPorDebajoDelCosto_DaGananciaNegativa()
        {
            TestScenario scenario = new();
            AddPurchase(scenario, Month.AddMonths(-1), 1, scenario.Chips, 700m);
            AddSale(scenario, Month.AddDays(2), (scenario.Chips, 2, 500m));

            MonthlyCloseResponse result = await scenario.ReportService.GetMonthlyCloseAsync(TestScenario.OwnerCuit, null, null, Month.Year, Month.Month);

            Assert.Equal(-400m, result.EstimatedProfit);
        }

        [Fact]
        public async Task GetMonthlyClose_SinCompras_GananciaCeroYCuentaLosProductos()
        {
            TestScenario scenario = new();
            AddSale(scenario, Month.AddDays(2), (scenario.Cola, 1, 1000m), (scenario.Chips, 1, 500m));

            MonthlyCloseResponse result = await scenario.ReportService.GetMonthlyCloseAsync(TestScenario.OwnerCuit, null, null, Month.Year, Month.Month);

            Assert.Equal(0m, result.EstimatedProfit);
            Assert.Equal(2, result.ProductsWithoutCost);
        }

        private static void AddPurchase(TestScenario scenario, DateTime date, int number, Product product, decimal unitCost)
        {
            Purchase purchase = new Purchase { Id = Guid.NewGuid(), Number = number, BranchId = scenario.Branch.Id, UserCuit = TestScenario.OwnerCuit, Date = date };
            purchase.Details.Add(new PurchaseDetail { PurchaseId = purchase.Id, ProductId = product.Id, Quantity = 10, UnitCost = unitCost });
            scenario.Purchases.Purchases.Add(purchase);
        }

        private static void AddSale(TestScenario scenario, DateTime date, params (Product Product, int Quantity, decimal UnitPrice)[] lines)
        {
            Sale sale = new Sale { Id = Guid.NewGuid(), BranchId = scenario.Branch.Id, UserCuit = TestScenario.OwnerCuit, Date = date };
            foreach ((Product product, int quantity, decimal unitPrice) in lines)
            {
                sale.Details.Add(new SaleDetail { SaleId = sale.Id, ProductId = product.Id, Quantity = quantity, UnitPrice = unitPrice });
            }

            sale.Total = sale.Details.Sum(d => d.Quantity * d.UnitPrice);
            scenario.Sales.Sales.Add(sale);
        }
    }
}
