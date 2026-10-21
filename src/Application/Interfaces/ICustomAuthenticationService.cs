using Application.Models;

namespace Application.Interfaces
{
    public interface ICustomAuthenticationService
    {
        // Valida las credenciales y devuelve el JWT del usuario
        string Authenticate(AuthenticationRequest request);
    }
}
