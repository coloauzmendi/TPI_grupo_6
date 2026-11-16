using Domain.Enums;
using Domain.Interfaces;

namespace Domain.Entities
{
    public class Branch : ISoftDelete
    {
        public Guid Id { get; set; }

        public string Locality { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public Industry Industry { get; set; }

        public Guid CompanyId { get; set; }

        // Baja lógica
        public bool IsActive { get; set; } = true;
    }
}