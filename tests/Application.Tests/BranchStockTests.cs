using Domain.Entities;
using Domain.Exceptions;

namespace Application.Tests
{
    public class BranchStockTests
    {
        private static BranchStock NewStock(int quantity, int minimum)
        {
            BranchStock stock = new BranchStock { BranchId = Guid.NewGuid(), ProductId = Guid.NewGuid(), MinimumStock = minimum };
            stock.SetQuantity(quantity);
            return stock;
        }

        [Fact]
        public void Add_SumaLaCantidad()
        {
            BranchStock stock = NewStock(5, 2);

            stock.Add(3);

            Assert.Equal(8, stock.Quantity);
        }

        [Fact]
        public void Remove_DescuentaLaCantidad()
        {
            BranchStock stock = NewStock(5, 2);

            stock.Remove(5);

            Assert.Equal(0, stock.Quantity);
        }

        [Fact]
        public void Remove_MasQueElStock_LanzaValidacionYNoCambia()
        {
            BranchStock stock = NewStock(5, 2);

            Assert.Throws<AppValidationException>(() => stock.Remove(6));
            Assert.Equal(5, stock.Quantity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void AddYRemove_CantidadNoPositiva_LanzaValidacion(int quantity)
        {
            BranchStock stock = NewStock(5, 2);

            Assert.Throws<AppValidationException>(() => stock.Add(quantity));
            Assert.Throws<AppValidationException>(() => stock.Remove(quantity));
            Assert.Equal(5, stock.Quantity);
        }

        [Fact]
        public void SetQuantity_Negativa_LanzaValidacion()
        {
            BranchStock stock = NewStock(5, 2);

            Assert.Throws<AppValidationException>(() => stock.SetQuantity(-1));
            Assert.Equal(5, stock.Quantity);
        }

        [Theory]
        [InlineData(1, 2, true)]
        [InlineData(2, 2, true)]
        [InlineData(3, 2, false)]
        public void IsLow_IgualOPorDebajoDelMinimo(int quantity, int minimum, bool expected)
        {
            Assert.Equal(expected, NewStock(quantity, minimum).IsLow);
        }
    }
}
