using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User? GetByCuit(string cuit);
        List<User> GetByBranch(Guid branchId);
        User Create(User newUser);
        void Update(User updatedUser);
        void Delete(User userToDelete);
    }
}
