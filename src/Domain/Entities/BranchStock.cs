using Domain.Interfaces;

namespace Domain.Entities
{
    public class BranchStock : ISoftDelete
    {
        public Guid BranchId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public int MinimumStock { get; set; }

        // Baja lógica: el producto deja de estar activo en la sucursal
        public bool IsActive { get; set; } = true;
    }
}