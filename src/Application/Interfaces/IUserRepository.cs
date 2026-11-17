using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        User? GetByCuit(string cuit);
        List<User> GetByBranch(Guid branchId);
    }
}
