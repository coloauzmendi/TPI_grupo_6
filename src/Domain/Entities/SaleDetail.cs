namespace Domain.Entities
{
    public class SaleDetail
    {
        public Guid SaleId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        // Precio al momento de la venta: no cambia si después se actualiza el precio del producto
        public decimal UnitPrice { get; set; }
    }
}