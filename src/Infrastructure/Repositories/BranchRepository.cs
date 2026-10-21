using Application.Interfaces;
using Domain.Entities;
using Infraestructure.Data;

namespace Infraestructure.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly ApplicationContext _context;

        public BranchRepository(ApplicationContext context)
        {
            _context = context;
        }

        public List<Branch> GetAll()
        {
            return _context.Branches.ToList();
        }

        public Branch? GetById(Guid id)
        {
            return _context.Branches.FirstOrDefault(x => x.Id == id);
        }

        public List<Branch> GetByCompany(Guid companyId)
        {
            return _context.Branches.Where(x => x.CompanyId == companyId).ToList();
        }

        public Branch Create(Branch newBranch)
        {
            _context.Branches.Add(newBranch);
            _context.SaveChanges();

            return newBranch;
        }

        public void Update(Branch updatedBranch)
        {
            _context.Branches.Update(updatedBranch);
            _context.SaveChanges();
        }

        public void Delete(Branch branchToDelete)
        {
            _context.Branches.Remove(branchToDelete);
            _context.SaveChanges();
        }
    }
}
