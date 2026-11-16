using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Models
{
    public class BranchStockCreateRequest
    {
        public Guid ProductId { get; set; }

        [Range(1, ValidationLimits.MaxQuantity, ErrorMessage = "La cantidad debe estar entre 1 y 100.000.")]
        public int Quantity { get; set; }

        [Range(0, ValidationLimits.MaxQuantity, ErrorMessage = "El stock mínimo debe estar entre 0 y 100.000.")]
        public int MinimumStock { get; set; }

        // Costo de la carga: por unidad o total, uno de los dos
        [Range(typeof(decimal), ValidationLimits.MinAmountText, ValidationLimits.MaxAmountText, ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El costo unitario debe estar entre 0,01 y 99.999.999,99.")]
        public decimal? UnitCost { get; set; }

        [Range(typeof(decimal), ValidationLimits.MinAmountText, ValidationLimits.MaxTotalAmountText, ParseLimitsInInvariantCulture = true,
            ErrorMessage = "El costo total debe estar entre 0,01 y 9.999.999.999,99.")]
        public decimal? TotalCost { get; set; }
    }

    public class BranchStockUpdateRequest
    {
        [Range(0, ValidationLimits.MaxQuantity, ErrorMessage = "La cantidad debe estar entre 0 y 100.000.")]
        public int Quantity { get; set; }

        [Range(0, ValidationLimits.MaxQuantity, ErrorMessage = "El stock mínimo debe estar entre 0 y 100.000.")]
        public int MinimumStock { get; set; }
    }

    public class BranchStockResponse
    {
        public Guid BranchId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int MinimumStock { get; set; }
    }

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
        public bool IsActive { get; set; }
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