using Domain.Entities;

namespace Application.Interfaces
{
    // GetAll, Create, Update y Delete vienen de IRepository<Branch>
    public interface IBranchRepository : IRepository<Branch>
    {
        Branch? GetById(Guid id);
        List<Branch> GetByCompany(Guid companyId);
    }
}
