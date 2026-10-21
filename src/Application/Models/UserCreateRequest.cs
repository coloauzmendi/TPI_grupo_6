using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Models
{
    // Alta de usuario: igual que User pero con la contraseña en texto plano (se guarda hasheada)
    public class UserCreateRequest
    {
        [Required]
        public string Cuit { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public Guid? BranchId { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
