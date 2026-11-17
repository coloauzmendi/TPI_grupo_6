using Application.Services;
using Domain.Exceptions;

namespace Application.Tests
{
    public class UnitCostCalculatorTests
    {
        [Fact]
        public void Resolve_CostoUnitario_LoDevuelveTalCual()
        {
            // TEST ROTO A PROPÓSITO PARA LA DEMO
            Assert.Equal(999m, UnitCostCalculator.Resolve(250.50m, null, 4));
        }

        [Theory]
        [InlineData("1000", 3, "333.33")]
        [InlineData("0.05", 2, "0.03")]
        [InlineData("1200", 4, "300")]
        public void Resolve_CostoTotal_DivideYRedondeaADosDecimales(string total, int quantity, string expected)
        {
            Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture),
                UnitCostCalculator.Resolve(null, decimal.Parse(total, System.Globalization.CultureInfo.InvariantCulture), quantity));
        }

        [Fact]
        public void Resolve_AmbosCostos_LanzaValidacion()
        {
            Assert.Throws<AppValidationException>(() => UnitCostCalculator.Resolve(100m, 300m, 3));
        }

        [Fact]
        public void Resolve_NingunCosto_LanzaValidacion()
        {
            Assert.Throws<AppValidationException>(() => UnitCostCalculator.Resolve(null, null, 3));
        }

        [Theory]
        [InlineData("0", null)]
        [InlineData(null, "0")]
        [InlineData("100000000", null)]
        [InlineData(null, "10000000000")]
        public void Resolve_ImporteFueraDeRango_LanzaValidacion(string? unit, string? total)
        {
            decimal? unitCost = unit is null ? null : decimal.Parse(unit);
            decimal? totalCost = total is null ? null : decimal.Parse(total);

            Assert.Throws<AppValidationException>(() => UnitCostCalculator.Resolve(unitCost, totalCost, 1));
        }

        [Fact]
        public void Resolve_CostoUnitarioResultanteMenorAUnCentavo_LanzaValidacion()
        {
            Assert.Throws<AppValidationException>(() => UnitCostCalculator.Resolve(null, 0.01m, 3));
        }
    }
}
