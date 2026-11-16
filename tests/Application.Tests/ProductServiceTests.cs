using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class ProductServiceTests
    {
        [Fact]
        public void Update_ProductoDeMiSucursal_ActualizaLosDatos()
        {
            TestScenario scenario = new();
            Product changes = new() { Name = " Gaseosa 2L ", Brand = "Cola", Category = "Bebidas", Price = 2500m, Industry = Industry.Kiosk };

            scenario.ProductService.Update(TestScenario.OwnerCuit, scenario.Cola.Id, changes);

            Assert.Equal("Gaseosa 2L", scenario.Cola.Name);
            Assert.Equal(2500m, scenario.Cola.Price);
        }

        [Fact]
        public void Update_ProductoQueNoEstaEnMisSucursales_LanzaNotAllowed()
        {
            TestScenario scenario = new();
            Product changes = new() { Name = "X", Brand = "X", Category = "X", Price = 1m };

            Assert.Throws<NotAllowedException>(() =>
                scenario.ProductService.Update(TestScenario.OutsiderCuit, scenario.Cola.Id, changes));
            Assert.Equal(1000m, scenario.Cola.Price);
        }

        [Fact]
        public void Create_PrecioNegativo_LanzaValidacion()
        {
            TestScenario scenario = new();
            Product product = new() { Name = "Agua", Brand = "Marca", Category = "Bebidas", Price = -1m };

            Assert.Throws<AppValidationException>(() => scenario.ProductService.Create(product));
        }

        [Fact]
        public void Delete_ProductoConVentas_LanzaValidacionYNoBorra()
        {
            TestScenario scenario = new();
            scenario.Products.WithMovements.Add(scenario.Cola.Id);

            Assert.Throws<AppValidationException>(() =>
                scenario.ProductService.Delete(TestScenario.OwnerCuit, scenario.Cola.Id));
            Assert.Contains(scenario.Cola, scenario.Products.Products);
        }

        [Fact]
        public void Delete_ProductoSinMovimientos_LoBorra()
        {
            TestScenario scenario = new();

            scenario.ProductService.Delete(TestScenario.OwnerCuit, scenario.Chips.Id);

            Assert.DoesNotContain(scenario.Chips, scenario.Products.Products);
        }
    }
}
