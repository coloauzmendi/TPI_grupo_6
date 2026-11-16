using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class PurchaseItemRequest
    {
        public Guid ProductId { get; set; }

        [Range(1, ValidationLimits.MaxQuantity, ErrorMessage = "La cantidad de cada producto debe estar entre 1 y 100.000.")]
        public int Quantity { get; set; }

        // Se indica el costo por unidad o el total de la línea, uno de los dos
        [Range(typeof(decimal), ValidationLimits.MinAmountText, ValidationLimits.MaxAmountText, ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El costo unitario debe estar entre 0,01 y 99.999.999,99.")]
        public decimal? UnitCost { get; set; }

        [Range(typeof(decimal), ValidationLimits.MinAmountText, ValidationLimits.MaxTotalAmountText, ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El costo total debe estar entre 0,01 y 9.999.999.999,99.")]
        public decimal? TotalCost { get; set; }
    }

    public class PurchaseRequest
    {
        [StringLength(100, ErrorMessage = "El proveedor no puede superar los 100 caracteres.")]
        public string? Supplier { get; set; }

        [Required(ErrorMessage = "La compra debe tener al menos un producto.")]
        [MinLength(1, ErrorMessage = "La compra debe tener al menos un producto.")]
        public List<PurchaseItemRequest> Items { get; set; } = new();
    }

    public class PurchaseItemResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        // Stock de la sucursal después de ingresar la mercadería
        public int NewStock { get; set; }
    }

    public class PurchaseResponse
    {
        public Guid Id { get; set; }
        // Número formateado: C-00012
        public string Code { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public DateTime Date { get; set; }
        public string? Supplier { get; set; }
        public decimal Total { get; set; }
        public List<PurchaseItemResponse> Items { get; set; } = new();
    }
}