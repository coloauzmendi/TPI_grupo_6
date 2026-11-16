using Domain.Interfaces;

namespace Domain.Entities
{
    public class Company : ISoftDelete
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Baja lógica
        public bool IsActive { get; set; } = true;

        public List<User> Owners { get; set; } = new();
    }
}