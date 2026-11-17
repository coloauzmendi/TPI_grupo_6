using System.ComponentModel.DataAnnotations;
using Application.Models;

namespace Application.Tests
{
    public class RequestValidationTests
    {
        private static List<ValidationResult> Validate(object request)
        {
            List<ValidationResult> results = new();
            Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
            return results;
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ProductRequest_NombreVacioOSoloEspacios_EsInvalido(string name)
        {
            ProductRequest request = new() { Name = name, Brand = "Generica", Category = "Bebidas", Price = 100m };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(ProductRequest.Name)));
        }

        [Theory]
        [InlineData("0")]
        [InlineData("100000000")]
        public void ProductRequest_PrecioFueraDeRango_EsInvalido(string price)
        {
            ProductRequest request = new() { Name = "Gaseosa", Brand = "Generica", Category = "Bebidas", Price = decimal.Parse(price) };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(ProductRequest.Price)));
        }

        [Fact]
        public void ProductCreateRequest_SinEmpresa_EsInvalido()
        {
            ProductCreateRequest request = new() { Name = "Gaseosa", Brand = "Generica", Category = "Bebidas", Price = 100m };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(ProductCreateRequest.CompanyId)));
        }

        [Fact]
        public void ProductRequest_MarcaDemasiadoLarga_EsInvalido()
        {
            ProductRequest request = new() { Name = "Gaseosa", Brand = new string('a', 51), Category = "Bebidas", Price = 100m };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(ProductRequest.Brand)));
        }

        [Fact]
        public void ProductRequest_Valido_NoTieneErrores()
        {
            ProductRequest request = new() { Name = "Gaseosa", Brand = "Generica", Category = "Bebidas", Price = 0.01m };

            Assert.Empty(Validate(request));
        }

        [Fact]
        public void BranchStockCreateRequest_CantidadCero_EsInvalido()
        {
            BranchStockCreateRequest request = new() { ProductId = Guid.NewGuid(), Quantity = 0, MinimumStock = 0 };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(BranchStockCreateRequest.Quantity)));
        }

        [Fact]
        public void BranchStockUpdateRequest_CantidadNegativa_EsInvalido()
        {
            BranchStockUpdateRequest request = new() { Quantity = -1, MinimumStock = 0 };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(BranchStockUpdateRequest.Quantity)));
        }

        [Fact]
        public void BranchStockUpdateRequest_CantidadCero_EsValido()
        {
            BranchStockUpdateRequest request = new() { Quantity = 0, MinimumStock = 100_000 };

            Assert.Empty(Validate(request));
        }

        [Fact]
        public void SaleRequest_SinItems_EsInvalido()
        {
            Assert.Contains(Validate(new SaleRequest()), r => r.MemberNames.Contains(nameof(SaleRequest.Items)));
        }

        [Fact]
        public void SaleItemRequest_CantidadNegativa_EsInvalido()
        {
            SaleItemRequest item = new() { ProductId = Guid.NewGuid(), Quantity = -5 };

            Assert.Contains(Validate(item), r => r.MemberNames.Contains(nameof(SaleItemRequest.Quantity)));
        }

        [Fact]
        public void PurchaseItemRequest_CostoCero_EsInvalido()
        {
            PurchaseItemRequest item = new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitCost = 0m };

            Assert.Contains(Validate(item), r => r.MemberNames.Contains(nameof(PurchaseItemRequest.UnitCost)));
        }

        [Fact]
        public void PurchaseRequest_ProveedorDemasiadoLargo_EsInvalido()
        {
            PurchaseRequest request = new()
            {
                Supplier = new string('a', 101),
                Items = { new PurchaseItemRequest { ProductId = Guid.NewGuid(), Quantity = 1, UnitCost = 1m } }
            };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(PurchaseRequest.Supplier)));
        }

        [Fact]
        public void UserCreateRequest_ContraseniaCorta_EsInvalido()
        {
            UserCreateRequest request = new() { Cuit = "20-33333333-3", FirstName = "Luis", LastName = "Diaz", Password = "12345" };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(UserCreateRequest.Password)));
        }

        [Fact]
        public void UserUpdateRequest_ContraseniaNuevaCorta_EsInvalido()
        {
            UserUpdateRequest request = new() { FirstName = "Luis", LastName = "Diaz", Password = "123" };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(UserUpdateRequest.Password)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void UserUpdateRequest_SinContrasenia_EsValido(string? password)
        {
            UserUpdateRequest request = new() { FirstName = "Luis", LastName = "Diaz", Password = password };

            Assert.Empty(Validate(request));
        }

        [Fact]
        public void UserUpdateRequest_ApellidoSoloEspacios_EsInvalido()
        {
            UserUpdateRequest request = new() { FirstName = "Luis", LastName = "   " };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(UserUpdateRequest.LastName)));
        }

        [Fact]
        public void CompanyRequest_NombreDemasiadoLargo_EsInvalido()
        {
            CompanyRequest request = new() { Name = new string('a', 101) };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(CompanyRequest.Name)));
        }

        [Fact]
        public void BranchStockCreateRequest_CostoTotalFueraDeRango_EsInvalido()
        {
            BranchStockCreateRequest request = new() { ProductId = Guid.NewGuid(), Quantity = 1, TotalCost = 10_000_000_000m };

            Assert.Contains(Validate(request), r => r.MemberNames.Contains(nameof(BranchStockCreateRequest.TotalCost)));
        }
    }
}
