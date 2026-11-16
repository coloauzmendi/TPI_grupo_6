using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    // Create y Update se heredan de Repository<Branch>
    public class BranchRepository : Repository<Branch>, IBranchRepository
    {
        public BranchRepository(ApplicationContext context) : base(context)
        {
        }

        // Solo las sucursales activas
        public override List<Branch> GetAll()
        {
            return _dbSet.Where(x => x.IsActive).ToList();
        }

        public Branch? GetById(Guid id)
        {
            return _dbSet.FirstOrDefault(x => x.Id == id && x.IsActive);
        }

        public List<Branch> GetByCompany(Guid companyId)
        {
            return _dbSet.Where(x => x.CompanyId == companyId && x.IsActive).ToList();
        }

        // Baja lógica
        public override void Delete(Branch branchToDelete)
        {
            branchToDelete.IsActive = false;
            Update(branchToDelete);
        }
    }
}
