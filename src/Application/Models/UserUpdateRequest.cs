using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    // Edición de un empleado: la contraseña es opcional (si viene vacía no se cambia)
    public class UserUpdateRequest : IValidatableObject
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        public string LastName { get; set; } = string.Empty;

        public Guid? BranchId { get; set; }

        public string? Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Password) && (Password.Length < 6 || Password.Length > 100))
            {
                yield return new ValidationResult("La contraseña debe tener entre 6 y 100 caracteres.", new[] { nameof(Password) });
            }
        }
    }
}
