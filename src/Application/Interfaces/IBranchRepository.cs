using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBranchRepository
    {
        List<Branch> GetAll();
        Branch? GetById(Guid id);
        List<Branch> GetByCompany(Guid companyId);
        Branch Create(Branch newBranch);
        void Update(Branch updatedBranch);
        void Delete(Branch branchToDelete);
    }
}
