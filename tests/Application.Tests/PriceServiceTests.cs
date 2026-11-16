using Application.Models;
using Application.Tests.Fakes;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class PriceServiceTests
    {
        [Fact]
        public void Update_PorCategoria_AplicaPorcentajeSoloALaCategoria()
        {
            TestScenario scenario = new();

            int updated = scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.Category,
                Target = "Snacks",
                Value = 15.5m
            });

            Assert.Equal(1, updated);
            Assert.Equal(577.50m, scenario.Chips.Price);
            Assert.Equal(1000m, scenario.Cola.Price);
            Assert.Equal(8000m, scenario.Hammer.Price);
            Assert.Equal(1, scenario.Products.UpdateRangeCalls);
        }

        [Fact]
        public void Update_Individual_AsignaElPrecioNuevo()
        {
            TestScenario scenario = new();

            int updated = scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.Individual,
                Target = scenario.Cola.Id.ToString(),
                Value = 1250m
            });

            Assert.Equal(1, updated);
            Assert.Equal(1250m, scenario.Cola.Price);
        }

        [Fact]
        public void Update_DescuentoMayorAlCienPorCiento_LanzaValidacion()
        {
            TestScenario scenario = new();

            Assert.Throws<AppValidationException>(() => scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.All,
                Value = -150m
            }));
            Assert.Equal(0, scenario.Products.UpdateRangeCalls);
        }

        [Fact]
        public void Update_UsuarioSinSucursales_LanzaValidacionPorqueNoHayProductos()
        {
            TestScenario scenario = new();

            Assert.Throws<AppValidationException>(() => scenario.PriceService.Update(TestScenario.OutsiderCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.All,
                Value = 10m
            }));
            Assert.Equal(1000m, scenario.Cola.Price);
        }
    }
}
