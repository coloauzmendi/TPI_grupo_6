using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    // Credenciales que se envían por body al endpoint de autenticación
    public class AuthenticationRequest
    {
        [Required(ErrorMessage = "El CUIT es obligatorio.")]
        public string Cuit { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}
