using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    // Credenciales que se envían por body al endpoint de autenticación
    public class AuthenticationRequest
    {
        [Required]
        public string Cuit { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
