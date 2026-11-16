using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPasswordHashService
    {
        string Hash(User user, string password);
        bool Verify(User user, string password);
    }
}
