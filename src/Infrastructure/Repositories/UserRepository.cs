using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    // Create y Update se heredan de Repository<User>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationContext context) : base(context)
        {
        }

        // Solo los usuarios activos
        public override List<User> GetAll()
        {
            return _dbSet.Where(x => x.IsActive).ToList();
        }

        // Devuelve también los usuarios dados de baja: el CUIT sigue ocupado
        public User? GetByCuit(string cuit)
        {
            return _dbSet.FirstOrDefault(x => x.Cuit == cuit);
        }

        public List<User> GetByBranch(Guid branchId)
        {
            return _dbSet.Where(x => x.BranchId == branchId && x.IsActive).ToList();
        }

        // Baja lógica: un usuario inactivo no puede iniciar sesión
        public override void Delete(User userToDelete)
        {
            userToDelete.IsActive = false;
            Update(userToDelete);
        }
    }
}
