using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.Models
{
    // Alta de usuario: igual que User pero con la contraseña en texto plano (se guarda hasheada)
    public class UserCreateRequest
    {
        [Required(ErrorMessage = "El CUIT es obligatorio.")]
        public string Cuit { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        public string LastName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public Guid? BranchId { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserResponse
    {
        public string Cuit { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public Guid? BranchId { get; set; }
    }
}
