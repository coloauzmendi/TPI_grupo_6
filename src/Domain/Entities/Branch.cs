using Domain.Enums;

namespace Domain.Entities
{
    public class Branch
    {
        public Guid Id { get; set; }

        public string Locality { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public Industry Industry { get; set; }

        public Guid CompanyId { get; set; }
    }
}
