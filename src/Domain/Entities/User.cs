using System.Text.Json.Serialization;
using Domain.Enums;

namespace Domain.Entities
{
    public class User
    {
        public string Cuit { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        // Hash de la contraseña; nunca se devuelve en las respuestas
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        // Solo para Employee: sucursal a la que está asignado (nula si es Owner)
        public Guid? BranchId { get; set; }

        // Solo para Owner: empresas de las que es dueño (N:M)
        [JsonIgnore]
        public List<Company> Companies { get; set; } = new();
    }
}
