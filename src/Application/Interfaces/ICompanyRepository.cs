using Domain.Entities;

namespace Application.Interfaces
{
    // GetAll, Create, Update y Delete vienen de IRepository<Company>
    public interface ICompanyRepository : IRepository<Company>
    {
        Company? GetById(Guid id);
        List<Company> GetByOwner(string ownerCuit);

        List<User> GetOwners(Guid companyId);
        void AddOwner(Guid companyId, User owner);
        void RemoveOwner(Guid companyId, string ownerCuit);
    }
}
