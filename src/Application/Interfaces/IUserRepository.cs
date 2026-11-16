using Domain.Entities;

namespace Application.Interfaces
{
    // GetAll, Create, Update y Delete vienen de IRepository<User>
    public interface IUserRepository : IRepository<User>
    {
        // La clave del usuario es el CUIT
        User? GetByCuit(string cuit);
        List<User> GetByBranch(Guid branchId);
    }
}
