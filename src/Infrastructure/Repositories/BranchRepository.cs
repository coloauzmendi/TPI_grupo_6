using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class BranchRepository : Repository<Branch>, IBranchRepository
    {
        public BranchRepository(ApplicationContext context) : base(context)
        {
        }

        public Branch? GetById(Guid id)
        {
            return _context.Branches.FirstOrDefault(x => x.Id == id && x.IsActive);
        }

        public List<Branch> GetByCompany(Guid companyId)
        {
            return _context.Branches.Where(x => x.CompanyId == companyId && x.IsActive).ToList();
        }
    }
}