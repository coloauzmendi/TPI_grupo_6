using System.Text.Json.Serialization;

namespace Domain.Entities
{
    public class BranchStock
    {
        public Guid BranchId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public int MinimumStock { get; set; }

        // Baja lógica: el producto deja de estar activo en la sucursal
        [JsonIgnore]
        public bool IsActive { get; set; } = true;
    }
}