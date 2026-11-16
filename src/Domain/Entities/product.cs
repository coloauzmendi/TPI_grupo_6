using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Product : ISoftDelete
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public Industry Industry { get; set; }

        public Guid CompanyId { get; set; }

        // Baja lógica
        public bool IsActive { get; set; } = true;
    }
}
