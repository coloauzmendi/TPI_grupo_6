using Domain.Entities;

namespace Application.Interfaces
{
    // Hasheo de contraseñas. La implementación (Infrastructure) usa el PasswordHasher de ASP.NET Identity,
    // así la capa Application no depende de ese paquete.
    public interface IPasswordService
    {
        string Hash(User user, string password);
    }
}
