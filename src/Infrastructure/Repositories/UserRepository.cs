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
            return _context.Users.Where(x => x.IsActive).ToList();
        }

        // Devuelve también los usuarios dados de baja: el CUIT sigue ocupado
        public User? GetByCuit(string cuit)
        {
            return _context.Users.FirstOrDefault(x => x.Cuit == cuit);
        }

        public List<User> GetByBranch(Guid branchId)
        {
            return _context.Users.Where(x => x.BranchId == branchId && x.IsActive).ToList();
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

        // Baja lógica
        public void Delete(User userToDelete)
        {
            userToDelete.IsActive = false;
            _context.Users.Update(userToDelete);
            _context.SaveChanges();
        }
    }
}