using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationContext _context;

        public UserRepository(ApplicationContext context)
        {
            _context = context;
        }

        public List<User> GetAll()
        {
            return _context.Users.ToList();
        }

        public User? GetByCuit(string cuit)
        {
            return _context.Users.FirstOrDefault(x => x.Cuit == cuit);
        }

        public List<User> GetByBranch(Guid branchId)
        {
            return _context.Users.Where(x => x.BranchId == branchId).ToList();
        }

        public User Create(User newUser)
        {
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return newUser;
        }

        public void Update(User updatedUser)
        {
            _context.Users.Update(updatedUser);
            _context.SaveChanges();
        }

        public void Delete(User userToDelete)
        {
            _context.Users.Remove(userToDelete);
            _context.SaveChanges();
        }
    }
}
