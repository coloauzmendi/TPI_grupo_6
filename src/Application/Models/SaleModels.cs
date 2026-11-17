using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class SaleItemRequest
    {
        public Guid ProductId { get; set; }

        [Range(1, ValidationLimits.MaxQuantity, ErrorMessage = "La cantidad de cada producto debe estar entre 1 y 100.000.")]
        public int Quantity { get; set; }
    }

    public class SaleRequest
    {
        [Required(ErrorMessage = "La venta debe tener al menos un producto.")]
        [MinLength(1, ErrorMessage = "La venta debe tener al menos un producto.")]
        public List<SaleItemRequest> Items { get; set; } = new();
    }

    public class SaleItemResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class SaleResponse
    {
        public Guid Id { get; set; }
        // Número formateado: V-000123
        public string Code { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public List<SaleItemResponse> Items { get; set; } = new();
    }

    // Resultado de registrar una venta: el comprobante y los avisos de stock bajo
    public class SaleResultResponse
    {
        public SaleResponse Sale { get; set; } = new();
        public List<string> Alerts { get; set; } = new();
    }
}