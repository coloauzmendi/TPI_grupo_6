using Application.Models;
using Application.Tests.Fakes;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.Tests
{
    public class ProductServiceTests
    {
        [Fact]
        public void Delete_DaDeBajaElProductoYSuStockActivo()
        {
            TestScenario scenario = new();
            Branch other = scenario.Branches.Create(new Branch { Id = Guid.NewGuid(), CompanyId = scenario.Company.Id, Locality = "Funes" });
            BranchStock otherStock = scenario.Stocks.Create(new BranchStock { BranchId = other.Id, ProductId = scenario.Cola.Id, Quantity = 4 });

            scenario.ProductService.Delete(scenario.Cola.Id);

            Assert.False(scenario.Cola.IsActive);
            Assert.False(scenario.StockOf(scenario.Cola).IsActive);
            Assert.False(otherStock.IsActive);
            Assert.True(scenario.StockOf(scenario.Chips).IsActive);
        }

        [Fact]
        public void Delete_ProductoInexistente_LanzaNotFound()
        {
            TestScenario scenario = new();

            Assert.Throws<NotFoundException>(() => scenario.ProductService.Delete(Guid.NewGuid()));
        }

        [Fact]
        public void Create_MismoNombreYMarcaActivo_LanzaConflictoYNoCrea()
        {
            TestScenario scenario = new();
            ProductRequest duplicate = new() { Name = "  GASEOSA ", Brand = "generica", Category = "Bebidas", Price = 1200m };

            Assert.Throws<ConflictException>(() => scenario.ProductService.Create(duplicate));
            Assert.Equal(3, scenario.Products.Products.Count);
        }

        [Fact]
        public void Create_MismoNombreYMarcaInactivo_LoReactivaConLosDatosNuevos()
        {
            TestScenario scenario = new();
            scenario.ProductService.Delete(scenario.Hammer.Id);
            ProductRequest again = new() { Name = "martillo", Brand = " Generica", Category = "Ferretería", Price = 9500m, Industry = Industry.Kiosk };

            ProductResponse result = scenario.ProductService.Create(again);

            Assert.Equal(scenario.Hammer.Id, result.Id);
            Assert.True(scenario.Hammer.IsActive);
            Assert.Equal("Martillo", result.Name);
            Assert.Equal("Ferretería", result.Category);
            Assert.Equal(9500m, result.Price);
            Assert.Equal(Industry.Kiosk, result.Industry);
            Assert.Equal(3, scenario.Products.Products.Count);
        }

        [Fact]
        public void Create_ProductoNuevo_LoAgrega()
        {
            TestScenario scenario = new();
            ProductRequest product = new() { Name = "Gaseosa", Brand = "Otra marca", Category = "Bebidas", Price = 900m };

            ProductResponse result = scenario.ProductService.Create(product);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Otra marca", result.Brand);
            Assert.Equal(4, scenario.Products.Products.Count);
        }
    }
}
