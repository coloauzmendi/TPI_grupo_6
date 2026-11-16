using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Company? GetById(Guid id);
        List<Company> GetByOwner(string ownerCuit);

        List<User> GetOwners(Guid companyId);
        void AddOwner(Guid companyId, User owner);
        void RemoveOwner(Guid companyId, string ownerCuit);
    }
}