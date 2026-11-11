using Domain.Enums;

namespace Application.Models
{
    // Stock de una sucursal junto con los datos del producto
    public class StockItemResponse
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Industry Industry { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
    }

    public class StockAlertResponse
    {
        public Guid BranchId { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
    }
}