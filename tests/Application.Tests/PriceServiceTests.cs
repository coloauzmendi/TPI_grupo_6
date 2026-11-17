using Application.Models;
using Application.Tests.Fakes;
using Domain.Entities;
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
        public void Update_PorcentajeMayorAMil_LanzaValidacion()
        {
            TestScenario scenario = new();

            Assert.Throws<AppValidationException>(() => scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.All,
                Value = 1001m
            }));
            Assert.Equal(0, scenario.Products.UpdateRangeCalls);
        }

        [Fact]
        public void Update_IndividualEnCero_LanzaValidacion()
        {
            TestScenario scenario = new();

            Assert.Throws<AppValidationException>(() => scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.Individual,
                Target = scenario.Cola.Id.ToString(),
                Value = 0m
            }));
            Assert.Equal(1000m, scenario.Cola.Price);
        }

        [Fact]
        public void Update_PrecioResultanteEnCero_LanzaValidacionYNoCambia()
        {
            TestScenario scenario = new();
            scenario.Chips.Price = 0.01m;

            Assert.Throws<AppValidationException>(() => scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.Category,
                Target = "Snacks",
                Value = -99m
            }));
            Assert.Equal(0, scenario.Products.UpdateRangeCalls);
        }

        [Fact]
        public void Update_Total_NoAfectaProductosDeOtraEmpresa()
        {
            TestScenario scenario = new();
            Product outsider = scenario.AddOutsiderProduct();

            int updated = scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.All,
                Value = 10m
            });

            Assert.Equal(3, updated);
            Assert.Equal(1100m, scenario.Cola.Price);
            Assert.Equal(1000m, outsider.Price);
        }

        [Fact]
        public void Update_IndividualConProductoDeOtraEmpresa_LanzaValidacionYNoCambia()
        {
            TestScenario scenario = new();
            Product outsider = scenario.AddOutsiderProduct();

            Assert.Throws<AppValidationException>(() => scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.Individual,
                Target = outsider.Id.ToString(),
                Value = 1m
            }));
            Assert.Equal(1000m, outsider.Price);
        }

        [Fact]
        public void Update_ConEmpresaAjena_LanzaNotAllowed()
        {
            TestScenario scenario = new();
            Product outsider = scenario.AddOutsiderProduct();

            Assert.Throws<NotAllowedException>(() => scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.All,
                Value = 10m,
                CompanyId = outsider.CompanyId
            }));
            Assert.Equal(1000m, outsider.Price);
            Assert.Equal(1000m, scenario.Cola.Price);
        }

        [Fact]
        public void Update_ConEmpresa_SoloAfectaEsaEmpresa()
        {
            TestScenario scenario = new();
            Product outsider = scenario.AddOutsiderProduct();
            scenario.Companies.GetById(outsider.CompanyId)!.Owners.Add(scenario.Users.GetByCuit(TestScenario.OwnerCuit)!);

            int updated = scenario.PriceService.Update(TestScenario.OwnerCuit, new PriceUpdateRequest
            {
                Mode = PriceMode.All,
                Value = 10m,
                CompanyId = scenario.Company.Id
            });

            Assert.Equal(3, updated);
            Assert.Equal(1100m, scenario.Cola.Price);
            Assert.Equal(1000m, outsider.Price);
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
