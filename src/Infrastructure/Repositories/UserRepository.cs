using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationContext context) : base(context)
        {
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
    }
}