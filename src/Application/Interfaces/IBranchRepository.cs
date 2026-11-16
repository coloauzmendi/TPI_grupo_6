using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBranchRepository : IRepository<Branch>
    {
        Branch? GetById(Guid id);
        List<Branch> GetByCompany(Guid companyId);
    }
}
